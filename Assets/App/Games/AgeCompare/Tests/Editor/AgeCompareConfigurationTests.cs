using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.AgeCompare;
using Lbs.MiniGames.Navigation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Lbs.MiniGames.Tests
{
    public sealed class AgeCompareConfigurationTests
    {
        [Test]
        public void CatalogAndBuildSettingsWireAgeCompareAsTheFinalLevel()
        {
            GameDefinition definition = AssetDatabase.LoadAssetAtPath<GameDefinition>("Assets/App/Catalog/Data/AgeCompareGame.asset");
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/App/Catalog/Data/MiniGameCatalog.asset");

            Assert.NotNull(definition);
            Assert.AreEqual(LevelSequenceRoute.AgeCompareGameId, definition.GameId);
            Assert.AreEqual("AgeCompare", definition.SceneName);
            Assert.IsTrue(definition.VisibleInHub);
            Assert.That(catalog.GetGames(definition.Category), Does.Contain(definition));
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Object>("Assets/App/Games/AgeCompare/AgeCompare.unity"));
            Assert.IsTrue(System.Array.Exists(EditorBuildSettings.scenes, scene => scene.enabled && scene.path == "Assets/App/Games/AgeCompare/AgeCompare.unity"));
            Assert.AreEqual(LevelSequenceRoute.AgeCompareGameId, LevelSequenceRoute.ShapePutSuccessTarget);
            Assert.IsTrue(LevelSequenceRoute.IsLogicSequenceGame(LevelSequenceRoute.AgeCompareGameId));
        }

        [Test]
        public void SceneProvidesTheConfiguredUGuiInputRoute()
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/App/Games/AgeCompare/AgeCompare.unity", OpenSceneMode.Additive);

            try
            {
                Assert.NotNull(Object.FindFirstObjectByType<AgeCompareGame>());
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
        public void PrincipalArtworkIsImportedAsASprite()
        {
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/AgeCompare/Art/Principal.png"));
        }
    }
}
