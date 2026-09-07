using System;
using System.Collections;
using System.Collections.Generic;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Lobby
{
    public sealed class LobbyController : MonoBehaviour, IAppScene
    {
        private enum DifficultyLevel
        {
            PrimariaBaja,
            PrimariaAlta,
            Secundaria
        }

        private sealed class DifficultyOptionView
        {
            public readonly RoundedSurface Surface;
            public readonly RoundedSurface InnerSurface;
            public readonly Text Label;

            public DifficultyOptionView(RoundedSurface surface, RoundedSurface innerSurface, Text label)
            {
                Surface = surface;
                InnerSurface = innerSurface;
                Label = label;
            }
        }

        private static readonly Color Purple = new(0.580f, 0.282f, 0.957f);
        private static readonly Color HeaderOverlay = new(0f, 0f, 0f, 0.38f);
        private static readonly Color Orange = new(1f, 0.718f, 0.251f);
        private static readonly Color DarkInk = new(0.141f, 0.102f, 0.208f);
        private static readonly Color ProfileCapsulePurple = new(0f, 0f, 0f, 0.40f);
        // Game-card title ink: a cool grey (not the near-black violet DarkInk). LogicLike card
        // labels sit at ~#5B5968 (saturation ~8%) — a soft grey, not black and not purple. We
        // darken it a touch to #4D4B5C so it stays legible from a distance on tablets/TVs.
        private static readonly Color CardTitleInk = new(0.302f, 0.294f, 0.361f);
        private static readonly Color Transparent = new(0f, 0f, 0f, 0f);
        private static readonly Color White = Color.white;
        private static readonly Color PalePurple = new(0.927f, 0.875f, 0.992f);
        private static readonly Color SoftPurple = new(0.780f, 0.643f, 0.980f);

        private const float OpeningDelaySeconds = 0.16f;
        private const float PlaceholderFeedbackDelaySeconds = 1f;
        // Reference-canvas values: 316x84 becomes 225x60 px at 1366x768.
        private const float ProfileCapsuleWidth = 316f;
        private const float ProfileCapsuleHeight = 84f;
        private const float AvatarFrameSize = 96f;
        // Keep the pill's left curve completely behind the circular avatar.
        private const float ProfileCapsuleAvatarOverlap = AvatarFrameSize * 0.60f;
        // Keep profile copy clear of the avatar's visible right edge.
        private const float ProfileTextAvatarGap = 16f;
        // Reference-canvas inset: 8.5 px on each side at the 1366 px-wide target.
        private const float AvatarImageInset = 12f;
        // Reference-canvas value: 422 px renders at ~300 px wide at 1366x768,
        // a 12% reduction from the previous 480 px / ~342 px pill.
        private const float DifficultyPillWidth = 422f;
        private const float DifficultyPillHeight = 60f;
        private const float DifficultySheetHeight = 620f;
        private const float DifficultySheetVisibleY = -64f;
        private const float DifficultySheetHiddenY = -684f;
        private const float DifficultySheetAnimationSeconds = 0.24f;
        // Reference-canvas values: 600 px renders as the approved 427 px-wide row at 1366 px.
        private const float DifficultyOptionWidth = 600f;
        private const float DifficultyOptionHeight = 96f;
        private const float DifficultyOptionGap = 20f;
        private const float DifficultyOptionsTopInset = 144f;
        private const float DifficultyOptionBorderThickness = 4f;
        private const string DifficultySheetTitleText = "Select a difficulty level";
        private static readonly Color DifficultyOptionSurface = White;

        // Scroll content / section layout (reference 1920x1080).
        // Nunito-Black's line box at font size 50 is ~68 reference pixels (its hhea
        // ascent/descent ratio is 1.364). Keep enough room so legacy UGUI Text does not
        // truncate the entire category label; Volte previously fit only by a narrow margin.
        private const float SectionHeaderHeight = 72f;
        // Width / height. Tuned so cards read as low and wide like LogicLike (~1.4) rather
        // than tall/square; keeps 3.5 visible and keeps the card from feeling oversized.
        private const float TileAspectRatio = 1.42f;
        private const float TileHorizontalGap = 44f;
        // Vertical space AFTER a card row (before the next section's label). Keeping it
        // larger than the gap below the label makes each label read as belonging to the
        // cards underneath it, not the ones above (LogicLike grouping).
        private const float SectionVerticalGap = 76f;
        // Vertical gap between a category label and the first card of its own row.
        private const float LabelToCardsGap = 36f;
        // Inner margin from the screen edge for section labels and the first/last cards in
        // each horizontal row. Sized for comfortable tablet/TV breathing room (not cramped).
        private const float ContentInnerMargin = 40f;
        // Left margin for section labels and the first card, aligned with the profile avatar
        // (avatar anchors at x=0.075 of the 1920 reference canvas -> 144px). Keeping this larger
        // than the right margin lines the category titles and the first card up with the profile,
        // at the cost of slightly narrower tiles (the 3.5-card carousel is preserved).
        private const float ContentLeftMargin = 144f;
        private const float MaxTileWidth = 620f;
        // Top inset so the first section starts clear of the translucent header band
        // (header is y>=0.84 -> ~173 reference px) with breathing room for the tiles.
        // Kept ~49px clear of the (now taller) header bottom edge.
        private const float ScrollTopPadding = 222f;
        // Card drop shadow: offset (down, in ref px) + low-opacity dark color.
        private const float ShadowOffsetY = 14f;
        private static readonly Color CardShadow = new(0.141f, 0.102f, 0.208f, 0.55f);

        [SerializeField] private GameCatalog catalog;
        [SerializeField] private Font interfaceFont;
        [Header("Card Title Font")]
        [Tooltip("Font used for game-card titles. Intentionally lighter than the interface font so cards read as calmer than the section headers.")]
        [SerializeField] private Font cardTitleFont;
        [Header("Wolfie Avatar")]
        [Tooltip("Optional non-interactive Wolfie sprite shown as a round header avatar.")]
        [SerializeField] private Sprite mascotSprite;
        [Header("Background Decorations")]
        [Tooltip("Low-opacity, slowly-rotating decorative shapes drawn behind the content. Order: blob, blob outline, spiral, hex, dots, ribbon, cloud, small blobs.")]
        [SerializeField] private Sprite[] backgroundDecorations;
        [Tooltip("Opacity multiplier applied to the whole decoration layer. Keep it low so the shapes read as subtle background texture, not content.")]
        [Range(0.02f, 0.40f)] [SerializeField] private float backgroundDecorOpacity = 0.22f;
        [Tooltip("Base rotation speed in degrees/second. Shapes alternate around this value; large shapes drift slower, small ones a touch faster.")]
        [SerializeField] private float backgroundDecorBaseSpeed = 2.5f;

        private AppServices services;

        // Difficulty selector state (design-only, it does not filter the catalog).
        private DifficultyLevel currentDifficulty = DifficultyLevel.PrimariaBaja;
        private Text difficultyLabel;
        private GameObject difficultySheet;
        private RectTransform difficultySheetPanel;
        private CanvasGroup difficultySheetCanvasGroup;
        private Text difficultySheetTitle;
        private Text difficultySheetTitlePrototype;
        private Coroutine difficultySheetAnimation;
        private bool difficultySheetTargetOpen;
        private readonly List<DifficultyOptionView> difficultyOptions = new();

        private bool launchInProgress;
        private bool loggedFontFallback;

        // Background decoration layer: each item rotates slowly on its own axis so the
        // scene feels alive without ever drawing attention away from the cards.
        private readonly List<BackgroundDecorView> backgroundDecor = new();

        private sealed class BackgroundDecorView
        {
            public readonly RectTransform Rect;
            public readonly float Weight;
            public readonly float DegreesPerSecond;

            public BackgroundDecorView(RectTransform rect, float weight, float degreesPerSecond)
            {
                Rect = rect;
                Weight = weight;
                DegreesPerSecond = degreesPerSecond;
            }
        }

        public void SetCatalog(GameCatalog gameCatalog)
        {
            catalog = gameCatalog;
        }

        public void SetInterfaceFont(Font font)
        {
            interfaceFont = font;
        }

        public void SetCardTitleFont(Font font)
        {
            cardTitleFont = font;
        }

        public void SetMascotSprite(Sprite sprite)
        {
            mascotSprite = sprite;
        }

        public void SetBackgroundDecorations(Sprite[] sprites)
        {
            backgroundDecorations = sprites;
        }

        public void SetBackgroundDecorOpacity(float opacity)
        {
            backgroundDecorOpacity = opacity;
        }

        public void Configure(AppServices appServices)
        {
            services = appServices;
            BuildInterface();
        }

        private void Update()
        {
            // Slow, gentle rotation on the background shapes (LogicLike-style). Time.deltaTime is
            // intentional: the decor is purely cosmetic, so it should pause with the game rather
            // than keep spinning behind a paused screen.
            for (int index = 0; index < backgroundDecor.Count; index++)
            {
                BackgroundDecorView decor = backgroundDecor[index];
                decor.Rect.Rotate(0f, 0f, decor.DegreesPerSecond * Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (difficultySheetAnimation != null)
            {
                StopCoroutine(difficultySheetAnimation);
                difficultySheetAnimation = null;
            }

            if (difficultySheetCanvasGroup != null)
            {
                difficultySheetCanvasGroup.interactable = false;
                difficultySheetCanvasGroup.blocksRaycasts = false;
            }

            if (difficultySheet != null)
            {
                Destroy(difficultySheet);
            }
        }

        private void BuildInterface()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Lobby requires a parent Canvas.", this);
                return;
            }

            RectTransform root = canvas.GetComponent<RectTransform>();
            Font font = ResolveInterfaceFont();

            // 1. Full-screen violet background.
            Image background = UiFactory.CreateImage(root, "HubBackground", Purple);
            UiFactory.Stretch(background.rectTransform, 0f);

            // 1b. Subtle rotating decoration layer, drawn right above the flat color but
            //     BELOW the scroll content. Shapes bleed off the screen edges so they read
            //     as ambient background texture rather than placed stickers; the cards and
            //     header render on top and keep the scene focused.
            CreateBackgroundDecorations(root);

            // 2. Scroll viewport stretches to the very top so category content scrolls
            //    UNDER the translucent header (the LogicLike effect). Built first so the
            //    header renders on top of it.
            GameObject mainArea = new("MainArea", typeof(RectTransform));
            mainArea.transform.SetParent(root, false);
            // The scrollable area runs edge-to-edge horizontally. Individual labels/cards
            // carry their own inner margin, so there is no outer purple gutter next to the
            // horizontal card rows (they reach the screen edges as they scroll).
            RectTransform mainAreaRoot = mainArea.GetComponent<RectTransform>();
            UiFactory.Anchor(mainAreaRoot, new Vector2(0f, 0f), new Vector2(1f, 1f));
            CreateScrollableCategoryList(mainAreaRoot, font);

            // 3. Header band: a translucent near-black overlay across the top. Drawn as a
            //    later sibling, so scrolled content passes beneath it (LogicLike style).
            //    Its raycastTarget blocks scroll-drag from starting on the fixed header;
            //    the difficulty pill and its dropdown are later siblings and stay clickable.
            Image headerBand = UiFactory.CreateImage(root, "HubHeaderBand", HeaderOverlay);
            UiFactory.Anchor(headerBand.rectTransform, new Vector2(0f, 0.84f), new Vector2(1f, 1f));
            headerBand.raycastTarget = true;

            GameObject headerObject = new("HubHeader", typeof(RectTransform));
            headerObject.transform.SetParent(root, false);
            RectTransform headerRoot = headerObject.GetComponent<RectTransform>();
            UiFactory.Anchor(headerRoot, new Vector2(0f, 0.84f), new Vector2(1f, 1f));
            headerRoot.transform.SetAsLastSibling();
            CreateHeaderContent(headerRoot, font);

            // 4. Pill difficulty selector: button in the header band + full-screen bottom sheet.
            CreateDifficultySelector(root, headerRoot, font);
        }

        private void CreateBackgroundDecorations(RectTransform root)
        {
            if (backgroundDecorations == null || backgroundDecorations.Length == 0)
            {
                return;
            }

            backgroundDecor.Clear();

            // One shared parent holds every shape so the layer can be faded as a whole and
            // is trivially redrawable. It is an early sibling of the canvas, so the scroll
            // content (created later) renders on top of it. It never intercepts raycasts.
            GameObject decorRoot = new("BackgroundDecor", typeof(RectTransform));
            decorRoot.transform.SetParent(root, false);
            RectTransform decorRect = decorRoot.GetComponent<RectTransform>();
            UiFactory.Stretch(decorRect, 0f);

            // Layout per shape: anchor box (fractions of the reference 1920x1080) and a
            // relative prominence weight. Some boxes intentionally stretch past the screen so
            // the shape bleeds off an edge — that is what makes it feel like part of the
            // environment, not a decal. All shapes keep raycastTarget off.
            //
            // Opacity model: each Image keeps full colour alpha; the whole layer is faded by
            // ONE CanvasGroup using backgroundDecorOpacity. The per-shape Weight only takes
            // that master fade and makes a given shape more/less prominent relative to the
            // rest, so the single serialized field stays the "how subtle" knob.
            DecorationSpec[] specs =
            {
                // blob_filled: large, top-left corner bleeding off left+top.
                new(new Vector2(-0.22f, 0.60f), new Vector2(0.26f, 1.08f), 1.0f, 1.0f),
                // blob_outline: outline hugging the upper-right.
                new(new Vector2(0.66f, 0.72f), new Vector2(1.10f, 1.16f), 0.7f, -1.0f),
                // spiral: hero shape, mid-left, a bit more presence.
                new(new Vector2(-0.10f, 0.18f), new Vector2(0.20f, 0.52f), 1.15f, 1.0f),
                // hex_outline: lower-left, half off-screen.
                new(new Vector2(-0.16f, -0.18f), new Vector2(0.20f, 0.34f), 0.7f, -1.0f),
                // dots: scattered texture across the centre-right.
                new(new Vector2(0.30f, 0.08f), new Vector2(0.82f, 0.62f), 0.55f, 0.8f),
                // ribbon: bottom band, bleeding off the bottom-right.
                new(new Vector2(0.52f, -0.34f), new Vector2(1.12f, 0.10f), 0.8f, 1.0f),
                // cloud: soft puff mid-right.
                new(new Vector2(0.72f, 0.30f), new Vector2(1.04f, 0.64f), 0.8f, -0.8f),
                // blobs_small: cluster, upper-centre, very faint.
                new(new Vector2(0.34f, 0.66f), new Vector2(0.66f, 0.92f), 0.45f, 0.8f),
            };

            // Build one view per shape, keeping its relative weight so the fade pass below can
            // align the opacity to the exact sprite that was added (a null sprite is skipped
            // without shifting later shapes). Bleed shapes off the edges for an ambient feel.
            float maxWeight = 1f;
            for (int index = 0; index < specs.Length && index < backgroundDecorations.Length; index++)
            {
                Sprite sprite = backgroundDecorations[index];
                if (sprite == null)
                {
                    continue;
                }

                DecorationSpec spec = specs[index];
                Image image = UiFactory.CreateImage(decorRect, "Decor_" + sprite.name, Color.white);
                image.sprite = sprite;
                image.preserveAspect = true;
                image.raycastTarget = false;

                UiFactory.Anchor(image.rectTransform, spec.MinAnchor, spec.MaxAnchor);

                // Alternate decay so larger shapes rotate slower than smaller ones; the sign
                // gives a bit of life so not everything spins the same direction.
                float falloff = 1f / Mathf.Max(1f, Mathf.Abs(spec.MaxAnchor.x - spec.MinAnchor.x));
                float speed = backgroundDecorBaseSpeed * spec.SpeedSign * (0.6f + falloff);
                backgroundDecor.Add(new BackgroundDecorView(image.rectTransform, spec.Weight, speed));
                maxWeight = Mathf.Max(maxWeight, spec.Weight);
            }

            // Fade the layer with ONE alpha. Per-shape weight makes the strongest shape land at
            // exactly backgroundDecorOpacity and lighter shapes sit proportionally below it, so
            // the single serialized field stays the master "how subtle" knob.
            float weightScale = 1f / maxWeight;
            CanvasGroup group = decorRoot.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;
            foreach (BackgroundDecorView decor in backgroundDecor)
            {
                Image image = decor.Rect.GetComponent<Image>();
                image.color = new Color(1f, 1f, 1f, backgroundDecorOpacity * decor.Weight * weightScale);
            }
        }

        private readonly struct DecorationSpec
        {
            public readonly Vector2 MinAnchor;
            public readonly Vector2 MaxAnchor;
            public readonly float Weight;
            public readonly float SpeedSign;

            public DecorationSpec(Vector2 minAnchor, Vector2 maxAnchor, float weight, float speedSign)
            {
                MinAnchor = minAnchor;
                MaxAnchor = maxAnchor;
                Weight = weight;
                SpeedSign = speedSign;
            }
        }

        private void CreateHeaderContent(RectTransform headerRoot, Font font)
        {
            RoundedSurface profileCapsule = UiFactory.CreateRoundedSurface(headerRoot, "ProfileCapsule", ProfileCapsulePurple, 999f, false);
            profileCapsule.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            profileCapsule.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            profileCapsule.rectTransform.pivot = new Vector2(0f, 0.5f);
            profileCapsule.rectTransform.anchoredPosition = new Vector2(
                ContentLeftMargin + AvatarFrameSize - ProfileCapsuleAvatarOverlap,
                0f);
            profileCapsule.rectTransform.sizeDelta = new Vector2(ProfileCapsuleWidth, ProfileCapsuleHeight);

            GameObject textBoundsObject = new("ProfileTextBounds", typeof(RectTransform));
            textBoundsObject.transform.SetParent(profileCapsule.rectTransform, false);
            RectTransform textBounds = textBoundsObject.GetComponent<RectTransform>();
            UiFactory.Stretch(textBounds, 0f);
            textBounds.offsetMin = new Vector2(ProfileCapsuleAvatarOverlap + ProfileTextAvatarGap, 4f);
            textBounds.offsetMax = new Vector2(-28f, -4f);

            Text profileName = UiFactory.CreateText(textBounds, "ProfileName", font, 42, TextAnchor.MiddleLeft, White);
            profileName.text = "Wolfie";
            profileName.fontStyle = FontStyle.Normal;
            profileName.resizeTextForBestFit = false;
            profileName.horizontalOverflow = HorizontalWrapMode.Overflow;
            profileName.verticalOverflow = VerticalWrapMode.Overflow;
            profileName.raycastTarget = false;
            UiFactory.Anchor(profileName.rectTransform, new Vector2(0f, 0.45f), new Vector2(1f, 1f));

            Font profileRangeFont = cardTitleFont != null ? cardTitleFont : font;
            Text profileRange = UiFactory.CreateText(textBounds, "ProfileRange", profileRangeFont, 26, TextAnchor.MiddleLeft, White);
            profileRange.text = "Lower Primary";
            profileRange.fontStyle = FontStyle.Normal;
            profileRange.resizeTextForBestFit = false;
            profileRange.horizontalOverflow = HorizontalWrapMode.Overflow;
            profileRange.verticalOverflow = VerticalWrapMode.Overflow;
            profileRange.raycastTarget = false;
            UiFactory.Anchor(profileRange.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.42f));

            CreateWolfieAvatar(headerRoot);
        }

        private void CreateWolfieAvatar(RectTransform headerRoot)
        {
            if (mascotSprite == null)
            {
                return;
            }

            GameObject frameObject = new("WolfieAvatar", typeof(RectTransform), typeof(CanvasRenderer), typeof(EllipseSurface));
            frameObject.transform.SetParent(headerRoot, false);
            RectTransform avatarFrame = frameObject.GetComponent<RectTransform>();
            avatarFrame.anchorMin = new Vector2(0f, 0.5f);
            avatarFrame.anchorMax = new Vector2(0f, 0.5f);
            avatarFrame.pivot = new Vector2(0.5f, 0.5f);
            avatarFrame.anchoredPosition = new Vector2(ContentLeftMargin + (AvatarFrameSize * 0.5f), 0f);
            avatarFrame.sizeDelta = Vector2.one * AvatarFrameSize;

            EllipseSurface avatarBackground = frameObject.GetComponent<EllipseSurface>();
            avatarBackground.color = SoftPurple;
            avatarBackground.raycastTarget = false;
            Mask mask = frameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            Image mascot = UiFactory.CreateImage(avatarFrame, "Mascot", Color.white);
            mascot.sprite = mascotSprite;
            mascot.preserveAspect = true;
            mascot.raycastTarget = false;
            UiFactory.Stretch(mascot.rectTransform, AvatarImageInset);

            VerifyWolfieAvatarFrame(avatarFrame, avatarBackground);
        }

        private static void VerifyWolfieAvatarFrame(RectTransform avatarFrame, EllipseSurface avatarBackground)
        {
            bool fixedSquare = avatarFrame.anchorMin == avatarFrame.anchorMax
                && Mathf.Approximately(avatarFrame.sizeDelta.x, avatarFrame.sizeDelta.y)
                && Mathf.Approximately(avatarFrame.rect.width, avatarFrame.rect.height);
            bool uniformScale = Mathf.Approximately(avatarFrame.lossyScale.x, avatarFrame.lossyScale.y);
            bool hasLayoutAncestor = false;
            for (Transform current = avatarFrame.parent; current != null; current = current.parent)
            {
                if (current.GetComponent<LayoutGroup>() != null || current.GetComponent<ContentSizeFitter>() != null)
                {
                    hasLayoutAncestor = true;
                    break;
                }
            }

            Debug.Assert(
                fixedSquare && uniformScale && !hasLayoutAncestor && avatarBackground != null,
                "WolfieAvatar must remain a fixed-size circular mask without layout-driven distortion.");
        }

        private void CreateDifficultySelector(RectTransform root, RectTransform headerRoot, Font font)
        {
            // Pill button in the header band.
            RoundedSurface pill = UiFactory.CreateRoundedSurface(headerRoot, "DifficultyPill", White, 999f, true);
            pill.rectTransform.anchorMin = new Vector2(0.95f, 0.5f);
            pill.rectTransform.anchorMax = new Vector2(0.95f, 0.5f);
            pill.rectTransform.pivot = new Vector2(1f, 0.5f);
            pill.rectTransform.anchoredPosition = Vector2.zero;
            pill.rectTransform.sizeDelta = new Vector2(DifficultyPillWidth, DifficultyPillHeight);
            Button pillButton = pill.gameObject.AddComponent<Button>();
            pillButton.targetGraphic = pill;

            Text pillLabel = UiFactory.CreateText(pill.rectTransform, "Label", font, 28, TextAnchor.MiddleCenter, DarkInk);
            pillLabel.text = "Difficulty: " + DifficultyLabel(currentDifficulty);
            pillLabel.raycastTarget = false;
            pillLabel.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            UiFactory.Stretch(pillLabel.rectTransform, 8f);
            difficultyLabel = pillLabel;
            pillButton.onClick.AddListener(OpenDifficultySheet);

            // The modal root covers the complete canvas, so its backdrop blocks all Hub input
            // while the sheet is visible. Its lower rounded corners remain below the viewport,
            // leaving only the strong top corners visible.
            GameObject sheetObject = new("DifficultySheet", typeof(RectTransform), typeof(CanvasGroup));
            sheetObject.transform.SetParent(root, false);
            sheetObject.transform.SetAsLastSibling();
            RectTransform sheetRoot = sheetObject.GetComponent<RectTransform>();
            UiFactory.Stretch(sheetRoot, 0f);
            difficultySheetCanvasGroup = sheetObject.GetComponent<CanvasGroup>();

            Image backdrop = UiFactory.CreateImage(sheetRoot, "Backdrop", new Color(0f, 0f, 0f, 0.54f));
            UiFactory.Stretch(backdrop.rectTransform, 0f);
            Button backdropButton = backdrop.gameObject.AddComponent<Button>();
            backdropButton.targetGraphic = backdrop;
            backdropButton.onClick.AddListener(CloseDifficultySheet);

            RoundedSurface panel = UiFactory.CreateRoundedSurface(sheetRoot, "PanelSurface", White, 56f, true);
            RectTransform panelRoot = panel.rectTransform;
            panelRoot.anchorMin = new Vector2(0f, 0f);
            panelRoot.anchorMax = new Vector2(1f, 0f);
            panelRoot.pivot = new Vector2(0.5f, 0f);
            panelRoot.anchoredPosition = new Vector2(0f, DifficultySheetHiddenY);
            panelRoot.sizeDelta = new Vector2(0f, DifficultySheetHeight);

            // Keep a generous tap target while rendering the close affordance as plain text.
            Image closeSurface = UiFactory.CreateImage(panelRoot, "CloseButton", Transparent);
            UiFactory.Anchor(closeSurface.rectTransform, new Vector2(0.03f, 0.844f), new Vector2(0.09f, 0.924f));
            Button closeButton = closeSurface.gameObject.AddComponent<Button>();
            closeButton.targetGraphic = closeSurface;
            Text closeLabel = UiFactory.CreateText(closeSurface.rectTransform, "Label", font, 52, TextAnchor.MiddleCenter, DarkInk);
            closeLabel.text = "×";
            closeLabel.resizeTextForBestFit = false;
            closeLabel.raycastTarget = false;
            UiFactory.Stretch(closeLabel.rectTransform, 0f);
            closeButton.onClick.AddListener(CloseDifficultySheet);

            DifficultyLevel[] levels =
            {
                DifficultyLevel.PrimariaBaja,
                DifficultyLevel.PrimariaAlta,
                DifficultyLevel.Secundaria
            };

            for (int index = 0; index < levels.Length; index++)
            {
                RoundedSurface option = UiFactory.CreateRoundedSurface(panelRoot, "Option", DifficultyOptionSurface, 28f, true);
                option.rectTransform.anchorMin = new Vector2(0.5f, 1f);
                option.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                option.rectTransform.pivot = new Vector2(0.5f, 1f);
                option.rectTransform.anchoredPosition = new Vector2(
                    0f,
                    -(DifficultyOptionsTopInset + (index * (DifficultyOptionHeight + DifficultyOptionGap))));
                option.rectTransform.sizeDelta = new Vector2(DifficultyOptionWidth, DifficultyOptionHeight);

                // Every row has the same two-layer construction. Selection only changes the
                // outer tint, exposing a thin brand-purple border without adding an icon.
                RoundedSurface optionInner = UiFactory.CreateRoundedSurface(
                    option.rectTransform,
                    "Surface",
                    DifficultyOptionSurface,
                    24f,
                    false);
                UiFactory.Stretch(optionInner.rectTransform, DifficultyOptionBorderThickness);

                Text optionLabel = UiFactory.CreateText(optionInner.rectTransform, "Label", font, 32, TextAnchor.MiddleCenter, new Color32(57, 62, 69, 255));
                optionLabel.text = DifficultyLabel(levels[index]);
                optionLabel.raycastTarget = false;
                UiFactory.Stretch(optionLabel.rectTransform, 12f);

                Button optionButton = option.gameObject.AddComponent<Button>();
                optionButton.targetGraphic = option;
                int capturedIndex = index;
                optionButton.onClick.AddListener(() => SelectDifficulty(capturedIndex));

                difficultyOptions.Add(new DifficultyOptionView(option, optionInner, optionLabel));
            }

            difficultySheetTitlePrototype = difficultyOptions[0].Label;
            difficultySheetTitle = CreateDifficultySheetTitle(panelRoot, difficultySheetTitlePrototype);

            sheetObject.SetActive(false);
            difficultySheet = sheetObject;
            difficultySheetPanel = panelRoot;
            RefreshDifficultyPresentation();
        }

        private static Text CreateDifficultySheetTitle(RectTransform panelRoot, Text visibleLabelPrototype)
        {
            // Use the same legacy UGUI Text, font asset, and material as a rendered option label.
            // The previous standalone title path did not have a visible-label rendering reference.
            Text title = UiFactory.CreateText(
                panelRoot,
                "DifficultySheetTitle",
                visibleLabelPrototype.font,
                42,
                TextAnchor.MiddleCenter,
                new Color32(57, 62, 69, 255));
            title.material = visibleLabelPrototype.material;
            title.fontStyle = visibleLabelPrototype.fontStyle;
            title.resizeTextForBestFit = visibleLabelPrototype.resizeTextForBestFit;
            title.resizeTextMinSize = visibleLabelPrototype.resizeTextMinSize;
            title.resizeTextMaxSize = 42;
            title.horizontalOverflow = visibleLabelPrototype.horizontalOverflow;
            title.verticalOverflow = visibleLabelPrototype.verticalOverflow;
            title.text = DifficultySheetTitleText;
            title.color = new Color32(57, 62, 69, 255);
            title.raycastTarget = false;
            title.enabled = true;
            title.gameObject.SetActive(true);
            title.rectTransform.localScale = Vector3.one;
            UiFactory.Anchor(title.rectTransform, new Vector2(0.14f, 0.844f), new Vector2(0.86f, 0.924f));
            title.transform.SetAsLastSibling();
            return title;
        }

        private static string DifficultyLabel(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.PrimariaAlta:
                    return "Upper Primary";
                case DifficultyLevel.Secundaria:
                    return "Secondary";
                default:
                    return "Lower Primary";
            }
        }

        private void OpenDifficultySheet()
        {
            SetDifficultySheetOpen(true);
        }

        private void CloseDifficultySheet()
        {
            SetDifficultySheetOpen(false);
        }

        private void SetDifficultySheetOpen(bool open)
        {
            if (difficultySheet == null || difficultySheetPanel == null || difficultySheetCanvasGroup == null)
            {
                return;
            }

            if (difficultySheetTargetOpen == open
                && difficultySheetAnimation == null
                && difficultySheet.activeSelf == open)
            {
                return;
            }

            difficultySheetTargetOpen = open;
            if (difficultySheetAnimation != null)
            {
                StopCoroutine(difficultySheetAnimation);
                difficultySheetAnimation = null;
            }

            if (open)
            {
                if (!difficultySheet.activeSelf)
                {
                    difficultySheetPanel.anchoredPosition = new Vector2(0f, DifficultySheetHiddenY);
                    difficultySheetCanvasGroup.alpha = 0f;
                    difficultySheet.SetActive(true);
                }

                difficultySheetCanvasGroup.interactable = true;
                difficultySheetCanvasGroup.blocksRaycasts = true;
                ValidateDifficultySheetTitle();
            }
            else if (!difficultySheet.activeSelf)
            {
                difficultySheetCanvasGroup.blocksRaycasts = false;
                return;
            }
            else
            {
                // Keep raycasts blocked until the reverse animation has fully left the screen.
                difficultySheetCanvasGroup.interactable = false;
                difficultySheetCanvasGroup.blocksRaycasts = true;
            }

            float targetY = open ? DifficultySheetVisibleY : DifficultySheetHiddenY;
            float targetAlpha = open ? 1f : 0f;
            difficultySheetAnimation = StartCoroutine(AnimateDifficultySheet(targetY, targetAlpha, !open));
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        private void ValidateDifficultySheetTitle()
        {
            Canvas.ForceUpdateCanvases();

            if (difficultySheetTitle == null || difficultySheetTitlePrototype == null || difficultySheetPanel == null)
            {
                Debug.Assert(false, "Difficulty sheet title was not constructed.");
                return;
            }

            RectTransform titleRect = difficultySheetTitle.rectTransform;
            bool activeAndEnabled = difficultySheetTitle.isActiveAndEnabled && difficultySheetTitle.gameObject.activeInHierarchy;
            bool opaqueTitleInk = difficultySheetTitle.color == new Color32(57, 62, 69, 255)
                && difficultySheetTitle.color.a > 0.99f;
            bool matchesVisibleLabelRendering = difficultySheetTitle.font != null
                && difficultySheetTitle.font == difficultySheetTitlePrototype.font
                && difficultySheetTitle.material != null
                && difficultySheetTitle.material == difficultySheetTitlePrototype.material;
            bool fixedPresentation = difficultySheetTitle.text == DifficultySheetTitleText
                && titleRect.parent == difficultySheetPanel
                && titleRect.GetSiblingIndex() == difficultySheetPanel.childCount - 1
                && titleRect.localScale == Vector3.one
                && Mathf.Approximately(titleRect.lossyScale.x, titleRect.lossyScale.y)
                && titleRect.rect.width > 0f
                && titleRect.rect.height > 0f;
            bool hasMaskAncestor = false;
            for (Transform current = titleRect.parent; current != null; current = current.parent)
            {
                if (current.GetComponent<Mask>() != null || current.GetComponent<RectMask2D>() != null)
                {
                    hasMaskAncestor = true;
                    break;
                }
            }

            Debug.Assert(
                activeAndEnabled
                && opaqueTitleInk
                && matchesVisibleLabelRendering
                && fixedPresentation
                && difficultySheetTitle.canvas != null
                && !hasMaskAncestor
                && IsRectWithinParentBounds(titleRect, difficultySheetPanel),
                "Difficulty sheet title must remain visible, opaque, unmasked, and inside the sheet.");
        }

        private static bool IsRectWithinParentBounds(RectTransform child, RectTransform parent)
        {
            Vector3[] corners = new Vector3[4];
            child.GetWorldCorners(corners);
            for (int index = 0; index < corners.Length; index++)
            {
                Vector3 localCorner = parent.InverseTransformPoint(corners[index]);
                if (!parent.rect.Contains(localCorner))
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerator AnimateDifficultySheet(float targetY, float targetAlpha, bool deactivateWhenFinished)
        {
            float startY = difficultySheetPanel.anchoredPosition.y;
            float startAlpha = difficultySheetCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < DifficultySheetAnimationSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / DifficultySheetAnimationSeconds);
                float easedProgress = progress * progress * (3f - (2f * progress));
                difficultySheetPanel.anchoredPosition = new Vector2(0f, Mathf.Lerp(startY, targetY, easedProgress));
                difficultySheetCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, easedProgress);
                yield return null;
            }

            difficultySheetPanel.anchoredPosition = new Vector2(0f, targetY);
            difficultySheetCanvasGroup.alpha = targetAlpha;
            difficultySheetAnimation = null;

            if (deactivateWhenFinished && !difficultySheetTargetOpen)
            {
                difficultySheetCanvasGroup.blocksRaycasts = false;
                difficultySheet.SetActive(false);
            }
        }

        private void SelectDifficulty(int index)
        {
            currentDifficulty = (DifficultyLevel)index;
            RefreshDifficultyPresentation();
            CloseDifficultySheet();
        }

        private void RefreshDifficultyPresentation()
        {
            if (difficultyLabel != null)
            {
                difficultyLabel.text = "Difficulty: " + DifficultyLabel(currentDifficulty);
            }

            for (int index = 0; index < difficultyOptions.Count; index++)
            {
                bool selected = index == (int)currentDifficulty;
                difficultyOptions[index].Surface.color = selected ? Purple : DifficultyOptionSurface;
                difficultyOptions[index].InnerSurface.color = selected ? PalePurple : DifficultyOptionSurface;
                difficultyOptions[index].Label.color = new Color32(57, 62, 69, 255);
            }
        }

        private void CreateScrollableCategoryList(RectTransform mainArea, Font font)
        {
            // Force a layout pass first so content/section rect widths are valid when we
            // read them below. Reading a rect before the canvas lays out yields 0/stale
            // widths, which pushes the section headers (and tile rows) off-screen left.
            Canvas.ForceUpdateCanvases();

            ScrollRect scrollRect = mainArea.gameObject.AddComponent<ScrollRect>();
            mainArea.gameObject.AddComponent<RectMask2D>();

            Image backdrop = mainArea.gameObject.AddComponent<Image>();
            backdrop.color = Transparent;
            backdrop.raycastTarget = true;

            // Content stretches across the full viewport width (anchors 0->1) so its
            // width always matches the scroll area without reading rect in a build frame.
            GameObject contentObject = new("Content", typeof(RectTransform));
            contentObject.transform.SetParent(mainArea, false);
            RectTransform content = contentObject.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0f, 1f);
            content.anchoredPosition = Vector2.zero;

            scrollRect.viewport = mainArea;
            scrollRect.content = content;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;

            // Viewport width is valid after the forced layout above; thread it into the
            // section builder instead of reading section.rect.width mid-construction.
            float viewportWidth = mainArea.rect.width;

            float yCursor = ScrollTopPadding;
            foreach (GameCategory category in catalog != null ? catalog.Categories : System.Array.Empty<GameCategory>())
            {
                if (category == null)
                {
                    continue;
                }

                List<GameDefinition> sectionGames = new();
                foreach (GameDefinition game in catalog.GetGames(category))
                {
                    // Hide test/legacy games from the Hub without removing their assets.
                    if (game != null && game.VisibleInHub)
                    {
                        sectionGames.Add(game);
                    }
                }

                yCursor = CreateCategorySection(content, category, sectionGames, viewportWidth, yCursor, font);
            }

            content.sizeDelta = new Vector2(0f, yCursor);
        }

        private float CreateCategorySection(
            RectTransform content,
            GameCategory category,
            List<GameDefinition> games,
            float viewportWidth,
            float yCursor,
            Font font)
        {
            int count = games.Count;

            // Size cards so ~3.5 columns fit across the row: 3 fully visible plus a clear
            // half-card peek of the 4th at the right edge (classic carousel affordance).
            // tileHeight derives from ratio, so the aspect ratio is preserved automatically.
            const float visibleColumns = 3.5f;
            float rowWidth = viewportWidth;
            float contentLeft = ContentLeftMargin;
            // Solve: contentLeft + visibleColumns*tileWidth + (visibleColumns-1)*gap == rowWidth.
            float tileWidth = (rowWidth - contentLeft - ((visibleColumns - 1f) * TileHorizontalGap)) / visibleColumns;
            tileWidth = Mathf.Min(tileWidth, MaxTileWidth);
            float tileHeight = tileWidth / TileAspectRatio;

            float sectionHeight = SectionHeaderHeight + tileHeight + SectionVerticalGap;

            GameObject sectionObject = new(category.DisplayName + "Section", typeof(RectTransform));
            sectionObject.transform.SetParent(content, false);
            RectTransform section = sectionObject.GetComponent<RectTransform>();
            section.anchorMin = new Vector2(0f, 1f);
            section.anchorMax = new Vector2(1f, 1f);
            section.pivot = new Vector2(0f, 1f);
            section.anchoredPosition = new Vector2(0f, -yCursor);
            section.sizeDelta = new Vector2(0f, sectionHeight);

            // Category label sits in its own full-width header band ABOVE the card row
            // (LogicLike style), so long names like "Matemáticas" always have room and
            // never collide with the cards. It is left-aligned with a small inner margin.
            Text header = UiFactory.CreateText(section, "SectionTitle", font, 50, TextAnchor.MiddleLeft, White);
            header.text = category.DisplayName;
            // Solid white, no synthetic stroke: the Black weight already gives the section
            // header enough body. Nunito has a taller line box than Volte, so allow vertical
            // overflow as a final guard against legacy UGUI truncating the whole line.
            header.fontStyle = FontStyle.Normal;
            header.resizeTextForBestFit = false;
            header.verticalOverflow = VerticalWrapMode.Overflow;
            header.raycastTarget = false;
            UiFactory.Anchor(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f));
            header.rectTransform.pivot = new Vector2(0f, 1f);
            // Align the category title's left edge with the logo/left margin (ContentLeftMargin)
            // while keeping the right inset at the smaller ContentInnerMargin.
            header.rectTransform.anchoredPosition = new Vector2(ContentLeftMargin, 0f);
            header.rectTransform.sizeDelta = new Vector2(-(ContentLeftMargin + ContentInnerMargin), SectionHeaderHeight);

            if (count == 0)
            {
                CreateSectionComingSoon(section, rowWidth, tileHeight, font);
            }
            else
            {
                // Horizontal scroll row. The content carries the left inner margin so the
                // row viewport itself stays edge-to-edge (no purple gap while scrolling).
                // A matching right margin keeps the last card off the screen edge at the end
                // of the scroll, so the list breathes on both sides.
                float contentRight = ContentInnerMargin;
                float rowContentWidth = contentLeft + (count * tileWidth) + ((count - 1) * TileHorizontalGap) + contentRight;
                RectTransform row = CreateHorizontalCardRow(section, tileWidth, tileHeight, rowContentWidth);
                for (int index = 0; index < count; index++)
                {
                    float x = contentLeft + (index * (tileWidth + TileHorizontalGap));
                    CreateGameCard(
                        games[index],
                        category.CategoryId == "logica",
                        category.CategoryId == "imagine-and-play",
                        row,
                        new Vector2(x, 0f),
                        new Vector2(tileWidth, tileHeight));
                }
            }

            return yCursor + sectionHeight;
        }

        private static RectTransform CreateHorizontalCardRow(
            RectTransform section,
            float tileWidth,
            float tileHeight,
            float contentWidth)
        {
            RectTransform rowViewport = new GameObject("CardRowViewport", typeof(RectTransform)).GetComponent<RectTransform>();
            rowViewport.SetParent(section, false);

            // Occupy exactly the card band of the section: below the label (top inset =
            // SectionHeaderHeight) and above the vertical gap (bottom inset = SectionVerticalGap),
            // with horizontal margins. Using offsetMin/offsetMax avoids negative-height sign bugs.
            rowViewport.anchorMin = new Vector2(0f, 0f);
            rowViewport.anchorMax = new Vector2(1f, 1f);
            // Top inset = below the section label. The viewport is tall enough to include
            // the card height PLUS the shadow offset, so the RectMask2D does not clip the
            // drop shadow's bottom edge. Runs edge-to-edge horizontally (no side margin).
            rowViewport.offsetMin = new Vector2(0f, SectionVerticalGap - LabelToCardsGap);
            rowViewport.offsetMax = new Vector2(0f, -(SectionHeaderHeight + LabelToCardsGap - ShadowOffsetY));

            // Transparent backdrop so the row receives drags; clips cards to its bounds.
            Image viewportImage = rowViewport.gameObject.AddComponent<Image>();
            viewportImage.color = Transparent;
            rowViewport.gameObject.AddComponent<RectMask2D>();

            ScrollRect rowScroll = rowViewport.gameObject.AddComponent<ScrollRect>();
            rowScroll.horizontal = true;
            rowScroll.vertical = false;
            rowScroll.movementType = ScrollRect.MovementType.Clamped;
            rowScroll.scrollSensitivity = 10f;
            rowScroll.viewport = rowViewport;

            // Gate the row's horizontal scroll behind the gesture's dominant axis so the
            // page's vertical scroll stays the primary scroll (nested-ScrollRect conflict).
            ScrollAxisRouter router = rowViewport.gameObject.AddComponent<ScrollAxisRouter>();
            router.Configure(rowScroll);

            RectTransform rowContent = new GameObject("CardRowContent", typeof(RectTransform)).GetComponent<RectTransform>();
            rowContent.SetParent(rowViewport, false);
            rowContent.anchorMin = new Vector2(0f, 1f);
            rowContent.anchorMax = new Vector2(0f, 1f);
            rowContent.pivot = new Vector2(0f, 1f);
            rowContent.anchoredPosition = Vector2.zero;
            // Taller than the card by the shadow offset so the drop shadow stays inside the
            // RectMask2D (otherwise the mask clips the shadow's bottom edge).
            rowContent.sizeDelta = new Vector2(contentWidth, tileHeight + ShadowOffsetY);

            rowScroll.content = rowContent;
            // Ensure the row starts scrolled to the far left (no card cut off at the edge)
            // regardless of the order the ScrollRect is wired up in.
            rowScroll.horizontalNormalizedPosition = 0f;
            return rowContent;
        }

        private static void CreateSectionComingSoon(RectTransform section, float width, float height, Font font)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(section, "ComingSoon", White, 30f, false);
            RectTransform rect = surface.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(width, height);

            Text label = UiFactory.CreateText(rect, "Label", font, 34, TextAnchor.MiddleCenter, DarkInk);
            label.text = "Coming soon";
            label.resizeTextForBestFit = false;
            label.raycastTarget = false;
            UiFactory.Stretch(label.rectTransform, 8f);
        }

        private void CreateGameCard(
            GameDefinition game,
            bool showSubjectLabel,
            bool useCoverThumbnail,
            RectTransform parent,
            Vector2 position,
            Vector2 size)
        {
            // Solid drop shadow behind the card, offset down a few reference pixels.
            // Created BEFORE the outline so it renders underneath (last sibling wins).
            RoundedSurface shadow = UiFactory.CreateRoundedSurface(parent, "CardShadow", CardShadow, 44f, false);
            shadow.rectTransform.anchorMin = new Vector2(0f, 1f);
            shadow.rectTransform.anchorMax = new Vector2(0f, 1f);
            shadow.rectTransform.pivot = new Vector2(0f, 1f);
            shadow.rectTransform.anchoredPosition = position + new Vector2(0f, -ShadowOffsetY);
            shadow.rectTransform.sizeDelta = size;

            RoundedSurface outline = UiFactory.CreateRoundedSurface(parent, "GameCard", White, 36f);
            RectTransform cardTransform = outline.rectTransform;
            cardTransform.anchorMin = new Vector2(0f, 1f);
            cardTransform.anchorMax = new Vector2(0f, 1f);
            cardTransform.pivot = new Vector2(0f, 1f);
            cardTransform.anchoredPosition = position;
            cardTransform.sizeDelta = size;

            CreateCardArtwork(cardTransform, game, useCoverThumbnail);

            Text title = UiFactory.CreateText(cardTransform, "Title", ResolveCardTitleFont(), 34, TextAnchor.MiddleCenter, CardTitleInk);
            title.text = showSubjectLabel ? game.HubSubjectLabel : game.VisibleName;
            // Card titles use the dedicated lighter weight (Nunito-Medium ~500) so cards read
            // calmer than the Black section headers. The single same-color stroke adds a hint of
            // weight, nudging the Medium toward a semibold feel. Best Fit stays off (moves with
            // the scroll, would re-layout every frame). Ink is a cool grey (CardTitleInk) to match
            // the LogicLike label tone rather than the near-black violet DarkInk.
            UiFactory.ApplySyntheticHeaderStroke(title, CardTitleInk);
            title.raycastTarget = false;
            UiFactory.Anchor(title.rectTransform, new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.24f));

            bool isPlaceholder = string.IsNullOrWhiteSpace(game.SceneName);
            RoundedSurface openingCue = UiFactory.CreateRoundedSurface(cardTransform, "OpeningCue", Orange, 20f, false);
            UiFactory.Anchor(
                openingCue.rectTransform,
                isPlaceholder ? new Vector2(0.24f, 0.42f) : new Vector2(0.40f, 0.42f),
                isPlaceholder ? new Vector2(0.76f, 0.58f) : new Vector2(0.60f, 0.58f));
            Text openingLabel = UiFactory.CreateText(openingCue.rectTransform, "Label", ResolveCardTitleFont(), 22, TextAnchor.MiddleCenter, DarkInk);
            openingLabel.text = isPlaceholder ? "Coming soon" : "Opening...";
            openingLabel.resizeTextForBestFit = false;
            openingLabel.raycastTarget = false;
            UiFactory.Stretch(openingLabel.rectTransform, 4f);

            GameCardFeedback feedback = outline.gameObject.AddComponent<GameCardFeedback>();
            // Keep the card surface white while pressed. Selection and the "Abriendo..." cue
            // remain active; only the temporary purple press tint is removed.
            feedback.Configure(outline, openingCue.gameObject, White, White);
            feedback.SelectionRequested += card => RequestLaunch(game, card);
        }

        private static void CreateCardArtwork(RectTransform cardTransform, GameDefinition game, bool useCoverThumbnail)
        {
            RoundedSurface artBackground = UiFactory.CreateRoundedSurface(cardTransform, "Artwork", PalePurple, 30f, false);
            // Edge-to-edge art (no inner padding) like LogicLike — the artwork fills the card
            // up to the bottom title band. Floor at 0.26 clears the title below.
            UiFactory.Anchor(artBackground.rectTransform, new Vector2(0f, 0.26f), new Vector2(1f, 1f));
            Mask artworkMask = artBackground.gameObject.AddComponent<Mask>();
            artworkMask.showMaskGraphic = false;

            if (game.Thumbnail != null)
            {
                Image thumbnail = UiFactory.CreateImage(artBackground.rectTransform, "Thumbnail", Color.white);
                thumbnail.sprite = game.Thumbnail;
                thumbnail.raycastTarget = false;
                if (useCoverThumbnail)
                {
                    ConfigureCoverThumbnail(thumbnail);
                }
                else
                {
                    // Preserve the existing contain behavior for all other categories.
                    thumbnail.preserveAspect = true;
                    UiFactory.Stretch(thumbnail.rectTransform, 0f);
                }

                return;
            }

            // Procedural fallback art (orange planet + purple node + soft node), kept as-is.
            RoundedSurface orangePlanet = UiFactory.CreateRoundedSurface(artBackground.rectTransform, "OrangePlanet", Orange, 999f, false);
            UiFactory.Anchor(orangePlanet.rectTransform, new Vector2(0.12f, 0.20f), new Vector2(0.37f, 0.70f));

            RoundedSurface purpleNode = UiFactory.CreateRoundedSurface(artBackground.rectTransform, "PurpleNode", Purple, 999f, false);
            UiFactory.Anchor(purpleNode.rectTransform, new Vector2(0.63f, 0.54f), new Vector2(0.79f, 0.84f));

            RoundedSurface softNode = UiFactory.CreateRoundedSurface(artBackground.rectTransform, "SoftNode", SoftPurple, 999f, false);
            UiFactory.Anchor(softNode.rectTransform, new Vector2(0.66f, 0.17f), new Vector2(0.86f, 0.46f));
        }

        private static void ConfigureCoverThumbnail(Image thumbnail)
        {
            RectTransform rectTransform = thumbnail.rectTransform;
            thumbnail.preserveAspect = true;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;

            // EnvelopeParent expands the image until it covers the masked artwork area while
            // preserving its aspect ratio. The centered pivot makes any overflow crop evenly.
            AspectRatioFitter fitter = thumbnail.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = thumbnail.sprite.rect.width / thumbnail.sprite.rect.height;
        }

        private void RequestLaunch(GameDefinition game, GameCardFeedback card)
        {
            if (game == null || launchInProgress)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(game.SceneName))
            {
                card.MarkOpening();
                StartCoroutine(ResetPlaceholderFeedback(card));
                return;
            }

            if (!game.IsValid())
            {
                return;
            }

            launchInProgress = true;
            card.MarkOpening();
            StartCoroutine(LaunchAfterFeedback(game, card));
        }

        private IEnumerator ResetPlaceholderFeedback(GameCardFeedback card)
        {
            yield return new WaitForSecondsRealtime(PlaceholderFeedbackDelaySeconds);
            card?.ResetOpening();
        }

        private IEnumerator LaunchAfterFeedback(GameDefinition game, GameCardFeedback card)
        {
            yield return new WaitForSecondsRealtime(OpeningDelaySeconds);

            try
            {
                // Future-ready difficulty plumbing: lobby auto-launches default difficulty without visible selector.
                GameLaunchRequest request = LobbyLaunchModel.CreateRequest(game);
                services.GameLauncher.Launch(request);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                launchInProgress = false;
                if (card != null)
                {
                    card.ResetOpening();
                }
            }
        }

        private Font ResolveInterfaceFont()
        {
            if (interfaceFont != null)
            {
                return interfaceFont;
            }

            if (!loggedFontFallback)
            {
                Debug.LogWarning("The interface font is not imported or assigned yet. The Lobby is using Unity's built-in fallback font.", this);
                loggedFontFallback = true;
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private Font ResolveCardTitleFont()
        {
            // Dedicated, lighter weight for game-card titles. Falls back to the interface font
            // (then the builtin) so a missing assignment still renders rather than erroring.
            if (cardTitleFont != null)
            {
                return cardTitleFont;
            }

            return ResolveInterfaceFont();
        }
    }
}
