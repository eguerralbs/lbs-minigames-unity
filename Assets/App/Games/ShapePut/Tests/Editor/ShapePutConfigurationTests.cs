using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.ShapePut;
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
    public sealed class ShapePutConfigurationTests
    {
        [Test]
        public void CatalogAndBuildSettingsWireShapePutAsTheFinalLevel()
        {
            GameDefinition definition = AssetDatabase.LoadAssetAtPath<GameDefinition>("Assets/App/Catalog/Data/ShapePutGame.asset");
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/App/Catalog/Data/MiniGameCatalog.asset");

            Assert.NotNull(definition);
            Assert.AreEqual(LevelSequenceRoute.ShapePutGameId, definition.GameId);
            Assert.AreEqual("ShapePut", definition.SceneName);
            Assert.IsTrue(definition.VisibleInHub);
            Assert.That(catalog.GetGames(definition.Category), Does.Contain(definition));
            Assert.AreEqual(LevelSequenceRoute.ShapePutGameId, LevelSequenceRoute.AppleMathSuccessTarget);
            Assert.IsTrue(LevelSequenceRoute.IsLogicSequenceGame(LevelSequenceRoute.ShapePutGameId));
            Assert.IsTrue(System.Array.Exists(EditorBuildSettings.scenes, scene => scene.enabled && scene.path == "Assets/App/Games/ShapePut/ShapePut.unity"));
        }

        [Test]
        public void SceneProvidesTheConfiguredUGuiInputRoute()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/App/Games/ShapePut/ShapePut.unity", OpenSceneMode.Additive);

            try
            {
                Canvas canvas = Object.FindFirstObjectByType<Canvas>();
                Assert.NotNull(Object.FindFirstObjectByType<ShapePutGame>());
                Assert.NotNull(canvas);
                Assert.AreEqual(new Vector2(1920f, 1080f), canvas.GetComponent<CanvasScaler>().referenceResolution);
                Assert.NotNull(Object.FindFirstObjectByType<EventSystem>());
                Assert.NotNull(Object.FindFirstObjectByType<StandaloneInputModule>());
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ShapeArtworkIsImportedAsSprites()
        {
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapePut/Art/Principal.png"));
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapePut/Art/Option1.png"));
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapePut/Art/Option2.png"));
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapePut/Art/Option3.png"));
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapePut/Art/Option4.png"));
        }
    }
}
