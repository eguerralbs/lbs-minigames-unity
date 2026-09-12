using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.ShortestRoute;
using Lbs.MiniGames.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Tests
{
    public sealed class ShortestRouteConfigurationTests
    {
        [Test]
        public void CatalogAndBuildSettings_WireTheShortestRouteFinalLevel()
        {
            GameDefinition definition = AssetDatabase.LoadAssetAtPath<GameDefinition>("Assets/App/Catalog/Data/ShortestRouteGame.asset");
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/App/Catalog/Data/MiniGameCatalog.asset");

            Assert.NotNull(definition);
            Assert.AreEqual(LevelSequenceRoute.ShortestRouteGameId, definition.GameId);
            Assert.AreEqual("ShortestRoute", definition.SceneName);
            Assert.IsTrue(definition.VisibleInHub);
            Assert.That(catalog.GetGames(definition.Category), Does.Contain(definition));
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Object>("Assets/App/Games/ShortestRoute/ShortestRoute.unity"));
            Assert.IsTrue(System.Array.Exists(EditorBuildSettings.scenes, scene => scene.enabled && scene.path == "Assets/App/Games/ShortestRoute/ShortestRoute.unity"));
            Assert.AreEqual(LevelSequenceRoute.ShortestRouteGameId, LevelSequenceRoute.WolfieFlasksSuccessTarget);
            Assert.IsTrue(LevelSequenceRoute.IsLogicSequenceGame(LevelSequenceRoute.ShortestRouteGameId));
        }

        [Test]
        public void Scene_ProvidesTheConfiguredUGuiInputRoute()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/App/Games/ShortestRoute/ShortestRoute.unity", OpenSceneMode.Additive);

            try
            {
                Assert.NotNull(Object.FindFirstObjectByType<ShortestRouteGame>());
                Assert.NotNull(Object.FindFirstObjectByType<Canvas>());
                Assert.NotNull(Object.FindFirstObjectByType<EventSystem>());
                Assert.NotNull(Object.FindFirstObjectByType<StandaloneInputModule>());
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [TestCase("Assets/App/Games/ShortestRoute/Art/Background.png")]
        [TestCase("Assets/App/Games/ShortestRoute/Art/Chart.png")]
        [TestCase("Assets/App/Games/ShortestRoute/Art/Character1.png")]
        [TestCase("Assets/App/Games/ShortestRoute/Art/Character2.png")]
        public void BoardArtwork_IsImportedAsASprite(string assetPath)
        {
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Sprite>(assetPath), assetPath);
        }
    }
}
