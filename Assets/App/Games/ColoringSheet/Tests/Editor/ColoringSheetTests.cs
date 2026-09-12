using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.ColoringSheet;
using Lbs.MiniGames.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lbs.MiniGames.Tests
{
    public sealed class ColoringSheetTests
    {
        private const string ScenePath = "Assets/App/Games/ColoringSheet/ColoringSheet.unity";
        private const string LineArtPath = "Assets/App/Games/ColoringSheet/Art/FoxLineArt.png";
        private const string RegionMapPath = "Assets/App/Games/ColoringSheet/Art/FoxRegionMap.png";

        [Test]
        public void StateSelectsPaintsRecolorsResetsAndRejectsInvalidRegions()
        {
            ColoringSheetState state = new(3, 4);
            state.SelectPalette(2);
            Assert.That(state.TryPaintRegion(1), Is.True);
            Assert.That(state.GetRegionPaletteIndex(1), Is.EqualTo(2));
            Assert.That(state.GetRegionPaletteIndex(0), Is.EqualTo(0));
            state.SelectPalette(3);
            state.TryPaintRegion(1);
            Assert.That(state.GetRegionPaletteIndex(1), Is.EqualTo(3));
            Assert.That(state.TryPaintRegion(-1), Is.False);
            Assert.That(state.TryPaintRegion(3), Is.False);
            state.Reset();
            Assert.That(Enumerable.Range(0, state.RegionCount).Select(state.GetRegionPaletteIndex), Is.All.EqualTo(0));
        }

        [Test]
        public void SourceLineArtAndRegionMapAreImportedAssignedAndAligned()
        {
            Sprite lineArt = AssetDatabase.LoadAssetAtPath<Sprite>(LineArtPath);
            Texture2D regionMap = AssetDatabase.LoadAssetAtPath<Texture2D>(RegionMapPath);
            TextureImporter importer = AssetImporter.GetAtPath(RegionMapPath) as TextureImporter;

            Assert.That(lineArt, Is.Not.Null);
            Assert.That(regionMap, Is.Not.Null);
            Assert.That(lineArt.texture.width, Is.EqualTo(regionMap.width));
            Assert.That(lineArt.texture.height, Is.EqualTo(regionMap.height));
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.isReadable, Is.True);
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.sRGBTexture, Is.False);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                ColoringSheetGame sceneGame = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ColoringSheetGame>(true)).Single();
                SerializedObject serializedGame = new(sceneGame);
                Assert.That(serializedGame.FindProperty("foxLineArt").objectReferenceValue, Is.SameAs(lineArt));
                Assert.That(serializedGame.FindProperty("foxRegionMap").objectReferenceValue, Is.SameAs(regionMap));
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }

        [Test]
        public void RegionMapContainsBackgroundAndMultipleClosedRegions()
        {
            Texture2D regionMap = AssetDatabase.LoadAssetAtPath<Texture2D>(RegionMapPath);
            Color32[] mapPixels = regionMap.GetPixels32();
            HashSet<byte> regionIds = new(mapPixels.Where(pixel => pixel.a != 0).Select(pixel => pixel.r));

            Assert.That(regionIds, Does.Contain(1));
            Assert.That(regionIds.Count, Is.GreaterThan(2));
            Assert.That(mapPixels.Any(pixel => pixel.a == 0), Is.True);
        }

        [Test]
        public void PainterDabAndStrokeStayInsideTheSelectedMapRegion()
        {
            ColoringSheetPainter painter = CreatePainter();
            Color32 red = new(255, 0, 0, 190);

            Assert.That(painter.PaintDab(new Vector2(-25f, 0f), 2, red), Is.True);
            Assert.That(painter.GetPixel(25, 50).a, Is.GreaterThan(0));
            Assert.That(painter.GetPixel(75, 50).a, Is.EqualTo(0));

            painter.PaintStroke(new Vector2(-35f, 0f), new Vector2(35f, 0f), 2, red);
            Assert.That(painter.GetPixel(75, 50).a, Is.EqualTo(0));
            Assert.That(painter.GetPixel(50, 50).a, Is.EqualTo(0));
        }

        [Test]
        public void PainterBackgroundStrokesNeverPaintFoxRegions()
        {
            ColoringSheetPainter painter = CreatePainter();
            Color32 blue = new(0, 0, 255, 190);

            Assert.That(painter.PaintDab(new Vector2(0f, 40f), painter.BackgroundRegionId, blue), Is.True);
            Assert.That(painter.GetPixel(50, 90).a, Is.GreaterThan(0));
            Assert.That(painter.GetPixel(50, 50).a, Is.EqualTo(0));
        }

        [Test]
        public void PaddedRegionMapKeepsOuterBoardPaintableAndPreservesArtworkRegionsAndBarriers()
        {
            Color32[] sourceMap =
            {
                new(2, 0, 0, 255), new(3, 0, 0, 255),
                default, new(4, 0, 0, 255)
            };
            RectInt artworkRect = new(3, 2, 4, 4);
            Color32[] paddedMap = ColoringSheetRegionMapBuilder.BuildPadded(sourceMap, 2, 2, 10, 8, artworkRect);
            ColoringSheetPainter painter = new(10, 8, new Vector2(10f, 8f), paddedMap, 1);
            Color32 blue = new(0, 0, 255, 190);

            Assert.That(painter.GetRegionAtPixel(0, 0), Is.EqualTo(painter.BackgroundRegionId));
            Assert.That(painter.GetRegionAtPixel(9, 7), Is.EqualTo(painter.BackgroundRegionId));
            Assert.That(painter.GetRegionAtPixel(3, 2), Is.EqualTo(2));
            Assert.That(painter.GetRegionAtPixel(5, 2), Is.EqualTo(3));
            Assert.That(painter.GetRegionAtPixel(3, 4), Is.EqualTo(-1));
            Assert.That(painter.GetRegionAtPixel(5, 4), Is.EqualTo(4));

            Assert.That(painter.PaintDab(new Vector2(-4.5f, -3.5f), painter.BackgroundRegionId, blue), Is.True);
            Assert.That(painter.GetPixel(0, 0).a, Is.GreaterThan(0));
            Assert.That(painter.GetPixel(3, 2).a, Is.EqualTo(0));
        }

        [Test]
        public void PaddedRegionMapPromotesOnlyUniqueInnerRegionEdges()
        {
            Color32[] sourceMap =
            {
                new(1, 0, 0, 255), new(1, 0, 0, 255), default, default, new(2, 0, 0, 255), new(2, 0, 0, 255),
            };
            Color32[] paddedMap = ColoringSheetRegionMapBuilder.BuildPadded(sourceMap, 6, 1, 6, 1, new RectInt(0, 0, 6, 1));
            ColoringSheetPainter painter = new(6, 1, new Vector2(6f, 1f), paddedMap, 1);
            Color32 red = new(255, 0, 0, 190);

            Assert.That(painter.GetRegionAtPixel(3, 0), Is.EqualTo(2));
            Assert.That(painter.GetRegionAtPixel(2, 0), Is.EqualTo(-1));
            Assert.That(painter.GetRegionAtPixel(1, 0), Is.EqualTo(painter.BackgroundRegionId));

            Assert.That(painter.PaintDab(new Vector2(0f, 0f), 2, red), Is.True);
            Assert.That(painter.GetPixel(3, 0).a, Is.GreaterThan(0));
            Assert.That(painter.GetPixel(2, 0).a, Is.EqualTo(0));

            Color32[] tiedMap = ColoringSheetRegionMapBuilder.BuildPadded(
                new[] { new Color32(2, 0, 0, 255), default, new Color32(3, 0, 0, 255) }, 3, 1, 3, 1, new RectInt(0, 0, 3, 1));
            Assert.That(new ColoringSheetPainter(3, 1, new Vector2(3f, 1f), tiedMap, 1).GetRegionAtPixel(1, 0), Is.EqualTo(-1));
        }

        [Test]
        public void PainterAndStateResetClearTheWorksheetSession()
        {
            ColoringSheetPainter painter = CreatePainter();
            ColoringSheetState state = new(3, 2);
            state.SelectPalette(1);
            state.TryPaintRegion(0);
            painter.PaintDab(new Vector2(-25f, 0f), 2, new Color32(255, 0, 0, 190));

            painter.Clear();
            state.Reset();

            Assert.That(painter.GetPixel(25, 50).a, Is.EqualTo(0));
            Assert.That(state.SelectedPaletteIndex, Is.EqualTo(0));
            Assert.That(state.GetRegionPaletteIndex(0), Is.EqualTo(0));
        }

        [Test]
        public void ProgressStoreRoundTripsTheEditablePixelsAndDerivedPreview()
        {
            string directory = CreateTemporaryDirectory();
            Texture2D paint = CreateTexture(4, 3, new Color32(10, 20, 30, 255));
            Texture2D preview = CreateTexture(4, 3, new Color32(40, 50, 60, 255));
            Sprite previewSprite = null;
            try
            {
                paint.SetPixel(2, 1, new Color32(200, 100, 50, 190));
                paint.Apply(false);
                ColoringSheetProgressStore store = new(ColoringSheetGame.StableGameId, ColoringSheetGame.ProgressSourceId, 4, 3, directory);

                Assert.That(store.Save(paint, preview), Is.True);
                Assert.That(store.TryLoadPixels(out Color32[] pixels), Is.True);
                Assert.That(pixels, Is.EqualTo(paint.GetPixels32()));
                Assert.That(store.TryCreatePreviewSprite(out previewSprite), Is.True);
                Assert.That(previewSprite.texture.GetPixels32(), Is.EqualTo(preview.GetPixels32()));
            }
            finally
            {
                DestroyTextureAndSprite(previewSprite);
                UnityEngine.Object.DestroyImmediate(paint);
                UnityEngine.Object.DestroyImmediate(preview);
                DeleteTemporaryDirectory(directory);
            }
        }

        [Test]
        public void ProgressStoreRejectsCorruptAndIncompatibleSavesAndDeletesThem()
        {
            string directory = CreateTemporaryDirectory();
            Texture2D paint = CreateTexture(2, 2, new Color32(255, 0, 0, 190));
            Texture2D preview = CreateTexture(2, 2, Color.white);
            try
            {
                ColoringSheetProgressStore store = new(ColoringSheetGame.StableGameId, ColoringSheetGame.ProgressSourceId, 2, 2, directory);
                Assert.That(store.Save(paint, preview), Is.True);

                ColoringSheetProgressStore incompatible = new(ColoringSheetGame.StableGameId, "other-artwork-v2", 2, 2, directory);
                Assert.That(incompatible.TryLoadPixels(out _), Is.False);
                Assert.That(incompatible.HasSavedProgress, Is.False);

                Assert.That(store.Save(paint, preview), Is.True);
                File.WriteAllBytes(store.SavePath, new byte[] { 1, 2, 3 });
                Assert.That(store.TryLoadPixels(out _), Is.False);
                Assert.That(store.HasSavedProgress, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(paint);
                UnityEngine.Object.DestroyImmediate(preview);
                DeleteTemporaryDirectory(directory);
            }
        }

        [Test]
        public void ProgressStoreRejectsDecodablePreviewWithIncompatibleDimensions()
        {
            string directory = CreateTemporaryDirectory();
            Texture2D paint = CreateTexture(2, 2, new Color32(255, 0, 0, 190));
            Texture2D invalidPreview = CreateTexture(3, 1, Color.white);
            try
            {
                ColoringSheetProgressStore store = new(ColoringSheetGame.StableGameId, ColoringSheetGame.ProgressSourceId, 2, 2, directory);
                WriteProgressFile(store.SavePath, paint.EncodeToPNG(), invalidPreview.EncodeToPNG(), 2, 2);

                Assert.That(store.TryCreatePreviewSprite(out _), Is.False);
                Assert.That(store.HasSavedProgress, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(paint);
                UnityEngine.Object.DestroyImmediate(invalidPreview);
                DeleteTemporaryDirectory(directory);
            }
        }

        [Test]
        public void ProgressStoreDeleteRemovesSavedPixelsAndPreview()
        {
            string directory = CreateTemporaryDirectory();
            Texture2D paint = CreateTexture(2, 2, new Color32(255, 0, 0, 190));
            Texture2D preview = CreateTexture(2, 2, Color.white);
            try
            {
                ColoringSheetProgressStore store = new(ColoringSheetGame.StableGameId, ColoringSheetGame.ProgressSourceId, 2, 2, directory);
                Assert.That(store.Save(paint, preview), Is.True);

                store.Delete();

                Assert.That(store.HasSavedProgress, Is.False);
                Assert.That(store.TryLoadPixels(out _), Is.False);
                Assert.That(store.TryCreatePreviewSprite(out _), Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(paint);
                UnityEngine.Object.DestroyImmediate(preview);
                DeleteTemporaryDirectory(directory);
            }
        }

        [Test]
        public void PreviewCompositionPlacesPaintOverWhiteAndLineArtOverPaint()
        {
            Texture2D paint = CreateTexture(4, 4, new Color32(0, 0, 0, 0));
            Texture2D lineArt = CreateTexture(2, 2, new Color32(0, 0, 0, 0));
            Texture2D preview = null;
            try
            {
                paint.SetPixel(1, 1, new Color32(255, 0, 0, 255));
                paint.Apply(false);
                lineArt.SetPixel(0, 0, new Color32(0, 0, 0, 255));
                lineArt.Apply(false);

                preview = ColoringSheetProgressStore.CreatePreview(paint, lineArt, new RectInt(1, 1, 2, 2));

                Assert.That(preview.GetPixel(0, 0), Is.EqualTo(Color.white));
                Assert.That(preview.GetPixel(1, 1), Is.EqualTo(Color.black));
            }
            finally
            {
                if (preview != null) UnityEngine.Object.DestroyImmediate(preview);
                UnityEngine.Object.DestroyImmediate(paint);
                UnityEngine.Object.DestroyImmediate(lineArt);
            }
        }

        [Test]
        public void CatalogBuildSettingsAndSceneExposeTheColoringPrototype()
        {
            GameDefinition definition = AssetDatabase.LoadAssetAtPath<GameDefinition>("Assets/App/Catalog/Data/ColoringSheetGame.asset");
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/App/Catalog/Data/MiniGameCatalog.asset");
            GameCategory category = AssetDatabase.LoadAssetAtPath<GameCategory>("Assets/App/Catalog/Data/MatematicasCategory.asset");
            Assert.That(definition.GameId, Is.EqualTo(ColoringSheetGame.StableGameId));
            Assert.That(definition.VisibleName, Is.EqualTo("Color the Fox"));
            Assert.That(definition.SceneName, Is.EqualTo("ColoringSheet"));
            Assert.That(definition.VisibleInHub, Is.True);
            Assert.That(definition.Category, Is.SameAs(category));
            Assert.That(catalog.GetGames(category), Does.Contain(definition));
            Assert.That(System.Array.Exists(EditorBuildSettings.scenes, scene => scene.enabled && scene.path == ScenePath), Is.True);
        }

        [Test]
        public void SceneBuildsOneLegacyInputRouteControlsPaintSurfaceAndLineArt()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            string progressDirectory = null;
            try
            {
                ColoringSheetGame game = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ColoringSheetGame>(true)).Single();
                GameSession session = new();
                progressDirectory = ConfigureGameForTest(game, session);
                Canvas.ForceUpdateCanvases();
                Transform[] hierarchy = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
                Assert.That(hierarchy.Select(item => item.GetComponent<Button>()).Count(button => button != null && button.name.StartsWith("Palette")), Is.EqualTo(12));
                Assert.That(hierarchy.Any(item => item.name == "Reset"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "Exit"), Is.True);
                RectTransform board = hierarchy.Single(item => item.name == "WorksheetBoard").GetComponent<RectTransform>();
                ColoringPaintSurface surface = hierarchy.Single(item => item.name == "PaintCanvas").GetComponent<ColoringPaintSurface>();
                Image lineArt = hierarchy.Single(item => item.name == "FoxLineArt").GetComponent<Image>();
                Assert.That(surface, Is.Not.Null);
                Assert.That(surface.rectTransform.rect.size, Is.EqualTo(board.rect.size));
                Assert.That(surface.raycastTarget, Is.True);
                Assert.That(lineArt?.sprite, Is.Not.Null);
                Assert.That(lineArt.rectTransform.rect.height, Is.EqualTo(680f));
                Assert.That(lineArt.rectTransform.rect.width / lineArt.rectTransform.rect.height, Is.EqualTo(lineArt.sprite.rect.width / lineArt.sprite.rect.height).Within(0.001f));
                Assert.That(lineArt.raycastTarget, Is.False);
                Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<EventSystem>(true)).Count(), Is.EqualTo(1));
                Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<StandaloneInputModule>(true)).Count(), Is.EqualTo(1));
            }
            finally { EditorSceneManager.CloseScene(scene, true); DeleteTemporaryDirectory(progressDirectory); }
        }

        [Test]
        public void PaletteCallbacksMoveAndResetTheVisibleSelectionMarker()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            string progressDirectory = null;
            try
            {
                ColoringSheetGame game = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ColoringSheetGame>(true)).Single();
                GameSession session = new();
                progressDirectory = ConfigureGameForTest(game, session);

                Button[] paletteButtons = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Button>(true))
                    .Where(button => button.name.StartsWith("Palette"))
                    .ToArray();
                Assert.That(paletteButtons.Count(button => button.transform.Find("SelectionMarker").gameObject.activeSelf), Is.EqualTo(1));
                Assert.That(paletteButtons.Single(button => button.name == "Palette0").transform.Find("SelectionMarker").gameObject.activeSelf, Is.True);

                Button selectedPalette = paletteButtons.Single(button => button.name == "Palette4");
                selectedPalette.onClick.Invoke();

                Assert.That(game.State.SelectedPaletteIndex, Is.EqualTo(4));
                Assert.That(paletteButtons.Count(button => button.transform.Find("SelectionMarker").gameObject.activeSelf), Is.EqualTo(1));
                Assert.That(selectedPalette.transform.Find("SelectionMarker").gameObject.activeSelf, Is.True);

                scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Button>(true))
                    .Single(button => button.name == "Reset")
                    .onClick.Invoke();

                Assert.That(game.State.SelectedPaletteIndex, Is.EqualTo(0));
                Assert.That(paletteButtons.Count(button => button.transform.Find("SelectionMarker").gameObject.activeSelf), Is.EqualTo(1));
                Assert.That(paletteButtons.Single(button => button.name == "Palette0").transform.Find("SelectionMarker").gameObject.activeSelf, Is.True);
            }
            finally { EditorSceneManager.CloseScene(scene, true); DeleteTemporaryDirectory(progressDirectory); }
        }

        private sealed class NoOpSceneLoader : ISceneLoader { public void Load(string sceneName) { } }

        private static string ConfigureGameForTest(ColoringSheetGame game, GameSession session)
        {
            string directory = CreateTemporaryDirectory();
            SerializedObject serializedGame = new(game);
            serializedGame.FindProperty("progressStorageDirectory").stringValue = directory;
            serializedGame.ApplyModifiedPropertiesWithoutUndo();
            game.Configure(new AppServices(session, new GameLauncher(session, new NoOpSceneLoader(), "Lobby")));
            return directory;
        }

        private static string CreateTemporaryDirectory()
        {
            string directory = Path.Combine(Path.GetTempPath(), "ColoringSheetTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            return directory;
        }

        private static void DeleteTemporaryDirectory(string directory)
        {
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        private static Texture2D CreateTexture(int width, int height, Color32 color)
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(Enumerable.Repeat(color, width * height).ToArray());
            texture.Apply(false);
            return texture;
        }

        private static void WriteProgressFile(string path, byte[] paintPng, byte[] previewPng, int width, int height)
        {
            using FileStream stream = new(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using BinaryWriter writer = new(stream);
            writer.Write("CSPS");
            writer.Write(1);
            writer.Write(ColoringSheetGame.StableGameId);
            writer.Write(ColoringSheetGame.ProgressSourceId);
            writer.Write(width);
            writer.Write(height);
            writer.Write(paintPng.Length);
            writer.Write(paintPng);
            writer.Write(previewPng.Length);
            writer.Write(previewPng);
        }

        private static void DestroyTextureAndSprite(Sprite sprite)
        {
            if (sprite == null) return;
            Texture2D texture = sprite.texture;
            UnityEngine.Object.DestroyImmediate(sprite);
            UnityEngine.Object.DestroyImmediate(texture);
        }

        private static ColoringSheetPainter CreatePainter()
        {
            Color32[] regionMap = Enumerable.Repeat(new Color32(1, 0, 0, 255), 100 * 100).ToArray();
            for (int y = 30; y <= 70; y++)
            {
                for (int x = 10; x < 50; x++) regionMap[y * 100 + x] = new Color32(2, 0, 0, 255);
                for (int x = 51; x < 90; x++) regionMap[y * 100 + x] = new Color32(3, 0, 0, 255);
                regionMap[y * 100 + 50] = default;
            }

            return new ColoringSheetPainter(100, 100, new Vector2(100f, 100f), regionMap, 3);
        }
    }
}
