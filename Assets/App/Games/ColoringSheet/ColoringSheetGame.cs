using System;
using System.Collections.Generic;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using Lbs.MiniGames.Shared.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.ColoringSheet
{
    /// <summary>Hosts a line-art worksheet with region-clipped pixel-pencil coloring.</summary>
    public sealed class ColoringSheetGame : MonoBehaviour, IAppScene
    {
        public const string StableGameId = "juega-aprende.colorear-zorro";
        public const string ProgressSourceId = "fox-worksheet-v1";
        public const int ProgressWidth = 1020;
        public const int ProgressHeight = 780;
        private const float WorksheetHeight = 680f;
        private static readonly Color[] Palette =
        {
            Color.white, new(0.95f, 0.25f, 0.33f), new(1f, 0.55f, 0.12f), new(1f, 0.84f, 0.16f),
            new(0.25f, 0.72f, 0.33f), new(0.14f, 0.63f, 0.82f), new(0.32f, 0.36f, 0.88f), new(0.60f, 0.30f, 0.82f),
            new(0.95f, 0.39f, 0.62f), new(0.45f, 0.25f, 0.14f), new(0.12f, 0.12f, 0.16f), new(0.62f, 0.74f, 0.83f)
        };
        [SerializeField] private Font interfaceFont;
        [SerializeField] private Sprite exitIcon;
        [SerializeField] private Sprite foxLineArt;
        [SerializeField] private Texture2D foxRegionMap;
        [SerializeField] private string progressStorageDirectory;

        private readonly Dictionary<int, Stroke> strokes = new();
        private readonly RoundedSurface[] paletteSelectionMarkers = new RoundedSurface[Palette.Length];
        private AppServices services;
        private ColoringSheetState state;
        private RectTransform worksheet;
        private Texture2D paintTexture;
        private ColoringSheetPainter painter;
        private ColoringSheetProgressStore progressStore;
        private RectInt artworkRect;
        private bool hasUnsavedChanges;
        private bool interfaceBuilt;

        public ColoringSheetState State => state;

        public void Configure(AppServices appServices)
        {
            services = appServices ?? throw new ArgumentNullException(nameof(appServices));
            if (!interfaceBuilt)
            {
                BuildInterface();
                interfaceBuilt = true;
            }
            state = new ColoringSheetState(painter.MaxRegionId + 1, Palette.Length);
            progressStore = new ColoringSheetProgressStore(StableGameId, ProgressSourceId, painter.Width, painter.Height, string.IsNullOrWhiteSpace(progressStorageDirectory) ? null : progressStorageDirectory);
            RestoreOrResetWorksheet();
        }

        private void BuildInterface()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) throw new InvalidOperationException("Coloring Sheet requires a parent Canvas.");
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            Font font = interfaceFont != null ? interfaceFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform root = new GameObject("ColoringSheetRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            root.SetParent(canvas.transform, false);
            UiFactory.Stretch(root, 0f);
            Image background = UiFactory.CreateImage(root, "WarmOrangeBackground", new Color(1f, 0.72f, 0.29f));
            UiFactory.Stretch(background.rectTransform, 0f);
            background.raycastTarget = false;

            Text title = UiFactory.CreateText(root, "Title", font, 58, TextAnchor.MiddleCenter, new Color(0.14f, 0.10f, 0.21f));
            title.text = "Color the Fox";
            title.resizeTextForBestFit = false;
            UiFactory.ApplySyntheticHeaderStroke(title, Color.white);
            Anchor(title.rectTransform, new Vector2(0.26f, 0.90f), new Vector2(0.74f, 0.98f));
            Text subtitle = UiFactory.CreateText(root, "PrototypeLabel", font, 24, TextAnchor.MiddleCenter, new Color(0.14f, 0.10f, 0.21f));
            subtitle.text = "FREE COLORING";
            Anchor(subtitle.rectTransform, new Vector2(0.26f, 0.855f), new Vector2(0.74f, 0.905f));

            RoundedSurface board = UiFactory.CreateRoundedSurface(root, "WorksheetBoard", Color.white, 42f, false);
            board.rectTransform.anchorMin = board.rectTransform.anchorMax = new Vector2(0.42f, 0.46f);
            board.rectTransform.sizeDelta = new Vector2(ProgressWidth, ProgressHeight);
            board.rectTransform.anchoredPosition = new Vector2(-160f, -20f);
            BuildPaintCanvas(board.rectTransform);
            RoundedSurface palettePanel = UiFactory.CreateRoundedSurface(root, "PalettePanel", Color.white, 34f, false);
            palettePanel.rectTransform.anchorMin = palettePanel.rectTransform.anchorMax = new Vector2(0.84f, 0.48f);
            palettePanel.rectTransform.sizeDelta = new Vector2(280f, 720f);
            palettePanel.rectTransform.anchoredPosition = new Vector2(0f, -15f);
            Text paletteLabel = UiFactory.CreateText(palettePanel.rectTransform, "PaletteLabel", font, 30, TextAnchor.MiddleCenter, new Color(0.14f, 0.10f, 0.21f));
            paletteLabel.text = "Colors";
            Anchor(paletteLabel.rectTransform, new Vector2(0.1f, 0.88f), new Vector2(0.9f, 0.97f));
            BuildPalette(palettePanel.rectTransform, font);
            Button reset = UiFactory.CreateRoundedButton(palettePanel.rectTransform, "Reset", font, "Reset", new Color(0.58f, 0.28f, 0.96f), Color.white, 26f);
            Anchor(reset.GetComponent<RectTransform>(), new Vector2(0.12f, 0.035f), new Vector2(0.88f, 0.13f));
            reset.onClick.AddListener(ResetWorksheet);
            LevelChrome chrome = LevelChromeFactory.Build(root, font, exitIcon, null, ReturnToLobby, null);
            chrome.HongButton.gameObject.SetActive(false);
        }

        private void BuildPaintCanvas(RectTransform board)
        {
            if (foxLineArt == null || foxRegionMap == null) throw new InvalidOperationException("Coloring Sheet requires assigned line art and a region map.");
            if (foxLineArt.texture.width != foxRegionMap.width || foxLineArt.texture.height != foxRegionMap.height)
                throw new InvalidOperationException("Coloring Sheet line art and region map dimensions must match.");

            Vector2 artworkSize = new(WorksheetHeight * foxRegionMap.width / foxRegionMap.height, WorksheetHeight);
            worksheet = new GameObject("PaintCanvas", typeof(RectTransform), typeof(CanvasRenderer), typeof(ColoringPaintSurface)).GetComponent<RectTransform>();
            worksheet.SetParent(board, false);
            UiFactory.Stretch(worksheet, 0f);
            int boardWidth = Mathf.RoundToInt(board.rect.width);
            int boardHeight = Mathf.RoundToInt(board.rect.height);
            int artworkWidth = Mathf.RoundToInt(artworkSize.x);
            int artworkHeight = Mathf.RoundToInt(artworkSize.y);
            artworkRect = new RectInt((boardWidth - artworkWidth) / 2, (boardHeight - artworkHeight) / 2, artworkWidth, artworkHeight);
            paintTexture = new Texture2D(boardWidth, boardHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            Color32[] paddedRegionMap = ColoringSheetRegionMapBuilder.BuildPadded(
                foxRegionMap.GetPixels32(), foxRegionMap.width, foxRegionMap.height, boardWidth, boardHeight, artworkRect);
            painter = new ColoringSheetPainter(paintTexture.width, paintTexture.height, new Vector2(boardWidth, boardHeight), paddedRegionMap);
            ColoringPaintSurface surface = worksheet.GetComponent<ColoringPaintSurface>();
            surface.texture = paintTexture;
            surface.color = Color.white;
            surface.raycastTarget = true;
            surface.Configure(BeginStroke, ContinueStroke, EndStroke);
            Image lineArt = UiFactory.CreateImage(worksheet, "FoxLineArt", Color.white);
            lineArt.rectTransform.anchorMin = lineArt.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            lineArt.rectTransform.sizeDelta = artworkSize;
            lineArt.sprite = foxLineArt;
            lineArt.preserveAspect = true;
            lineArt.raycastTarget = false;
            ApplyPaint();
        }

        private void BuildPalette(RectTransform parent, Font font)
        {
            for (int index = 0; index < Palette.Length; index++)
            {
                int paletteIndex = index;
                Button button = UiFactory.CreateRoundedButton(parent, $"Palette{index}", font, string.Empty, Palette[index], new Color(0.14f, 0.10f, 0.21f), 22f);
                RectTransform rect = button.GetComponent<RectTransform>();
                int column = index % 2;
                int row = index / 2;
                rect.anchorMin = rect.anchorMax = new Vector2(0.30f + column * 0.40f, 0.79f - row * 0.11f);
                rect.sizeDelta = new Vector2(82f, 62f);
                RoundedSurface marker = UiFactory.CreateRoundedSurface(rect, "SelectionMarker", new Color(0.14f, 0.10f, 0.21f), 18f, false);
                marker.OutlineThickness = 6f;
                UiFactory.Stretch(marker.rectTransform, 3f);
                marker.gameObject.SetActive(index == 0);
                paletteSelectionMarkers[index] = marker;
                button.onClick.AddListener(() => SelectPalette(paletteIndex));
            }
        }

        private void SelectPalette(int paletteIndex)
        {
            state.SelectPalette(paletteIndex);
            RefreshPaletteSelectionMarker();
        }

        private void BeginStroke(PointerEventData eventData)
        {
            if (!TryGetWorksheetPoint(eventData, out Vector2 point)) return;
            int activeRegion = painter.GetRegionAt(point);
            if (activeRegion < 0) return;
            state.TryPaintRegion(activeRegion);
            strokes[eventData.pointerId] = new Stroke(activeRegion, point);
            if (painter.PaintDab(point, activeRegion, GetBrushColor())) MarkPaintChanged();
        }

        private void ContinueStroke(PointerEventData eventData)
        {
            if (!strokes.TryGetValue(eventData.pointerId, out Stroke stroke) || !TryGetWorksheetPoint(eventData, out Vector2 point)) return;
            if (painter.PaintStroke(stroke.lastPoint, point, stroke.regionIndex, GetBrushColor())) MarkPaintChanged();
            strokes[eventData.pointerId] = new Stroke(stroke.regionIndex, point);
        }

        private void EndStroke(PointerEventData eventData)
        {
            if (strokes.Remove(eventData.pointerId)) SaveProgress();
        }

        private bool TryGetWorksheetPoint(PointerEventData eventData, out Vector2 point) => RectTransformUtility.ScreenPointToLocalPointInRectangle(worksheet, eventData.position, eventData.pressEventCamera, out point);
        private Color32 GetBrushColor() { Color32 color = Palette[state.SelectedPaletteIndex]; color.a = 190; return color; }

        private void ResetWorksheet()
        {
            state.Reset();
            RefreshPaletteSelectionMarker();
            strokes.Clear();
            painter?.Clear();
            ApplyPaint();
            hasUnsavedChanges = false;
            progressStore?.Delete();
        }

        private void RestoreOrResetWorksheet()
        {
            state.Reset();
            RefreshPaletteSelectionMarker();
            strokes.Clear();
            painter.Clear();
            if (progressStore.TryLoadPixels(out Color32[] pixels)) Array.Copy(pixels, painter.Pixels, pixels.Length);
            ApplyPaint();
            hasUnsavedChanges = false;
        }

        private void MarkPaintChanged()
        {
            hasUnsavedChanges = true;
            ApplyPaint();
        }

        private void SaveProgress()
        {
            if (!hasUnsavedChanges || progressStore == null || paintTexture == null || foxLineArt == null) return;
            Texture2D preview = null;
            try
            {
                preview = ColoringSheetProgressStore.CreatePreview(paintTexture, foxLineArt.texture, artworkRect);
                if (progressStore.Save(paintTexture, preview)) hasUnsavedChanges = false;
            }
            finally
            {
                if (preview != null) Destroy(preview);
            }
        }

        private void ApplyPaint()
        {
            if (paintTexture == null || painter == null) return;
            paintTexture.SetPixels32(painter.Pixels);
            paintTexture.Apply(false);
        }

        private void RefreshPaletteSelectionMarker()
        {
            for (int index = 0; index < paletteSelectionMarkers.Length; index++)
            {
                if (paletteSelectionMarkers[index] != null) paletteSelectionMarkers[index].gameObject.SetActive(index == state.SelectedPaletteIndex);
            }
        }

        private void OnApplicationPause(bool paused) { if (paused) SaveProgress(); }
        private void OnApplicationFocus(bool hasFocus) { if (!hasFocus) SaveProgress(); }
        private void OnDisable() { SaveProgress(); strokes.Clear(); }

        private void OnDestroy()
        {
            strokes.Clear();
            SaveProgress();
            if (paintTexture != null) Destroy(paintTexture);
        }

        private void ReturnToLobby()
        {
            SaveProgress();
            strokes.Clear();
            services?.GameLauncher?.ShowLobby();
        }

        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private readonly struct Stroke
        {
            public Stroke(int regionIndex, Vector2 lastPoint) { this.regionIndex = regionIndex; this.lastPoint = lastPoint; }
            public readonly int regionIndex;
            public readonly Vector2 lastPoint;
        }
    }
}
