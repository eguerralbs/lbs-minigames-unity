using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.AppleMath;
using Lbs.MiniGames.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Tests
{
    public sealed class AppleMathConfigurationTests
    {
        [Test]
        public void CatalogAndBuildSettingsWireAppleMathAsTheFinalLevel()
        {
            GameDefinition definition = AssetDatabase.LoadAssetAtPath<GameDefinition>("Assets/App/Catalog/Data/AppleMathGame.asset");
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/App/Catalog/Data/MiniGameCatalog.asset");

            Assert.NotNull(definition);
            Assert.AreEqual(LevelSequenceRoute.AppleMathGameId, definition.GameId);
            Assert.AreEqual("AppleMath", definition.SceneName);
            Assert.IsTrue(definition.VisibleInHub);
            Assert.That(catalog.GetGames(definition.Category), Does.Contain(definition));
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Object>("Assets/App/Games/AppleMath/AppleMath.unity"));
            Assert.IsTrue(System.Array.Exists(EditorBuildSettings.scenes, scene => scene.enabled && scene.path == "Assets/App/Games/AppleMath/AppleMath.unity"));
            Assert.AreEqual(LevelSequenceRoute.AppleMathGameId, LevelSequenceRoute.ShortestRouteSuccessTarget);
            Assert.IsTrue(LevelSequenceRoute.IsLogicSequenceGame(LevelSequenceRoute.AppleMathGameId));
        }

        [Test]
        public void SceneProvidesTheConfiguredUGuiInputRoute()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/App/Games/AppleMath/AppleMath.unity", OpenSceneMode.Additive);

            try
            {
                Assert.NotNull(Object.FindFirstObjectByType<AppleMathGame>());
                Assert.NotNull(Object.FindFirstObjectByType<Canvas>());
                Assert.NotNull(Object.FindFirstObjectByType<EventSystem>());
                Assert.NotNull(Object.FindFirstObjectByType<StandaloneInputModule>());
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void EquationArtworkIsImportedAsASprite()
        {
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/AppleMath/Art/Principal.png"));
        }
    }
}
