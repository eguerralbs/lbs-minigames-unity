using System.Linq;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Catalog;
using Lbs.MiniGames.Games.Memorama;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lbs.MiniGames.Tests
{
    public sealed class MemoramaSceneIntegrationTests
    {
        private const string ScenePath = "Assets/App/Games/Memorama/Memorama.unity";

        [Test]
        public void CatalogBuildSettingsAndSceneExposeTheDirectHubGame()
        {
            GameDefinition definition = AssetDatabase.LoadAssetAtPath<GameDefinition>("Assets/App/Catalog/Data/Placeholder.juega-aprende.memoria-animales.asset");
            GameCatalog catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/App/Catalog/Data/MiniGameCatalog.asset");
            GameCategory category = AssetDatabase.LoadAssetAtPath<GameCategory>("Assets/App/Catalog/Data/MatematicasCategory.asset");

            Assert.That(definition.GameId, Is.EqualTo(MemoramaGame.StableGameId));
            Assert.That(definition.SceneName, Is.EqualTo("Memorama"));
            Assert.That(definition.VisibleInHub, Is.True);
            Assert.That(definition.Category, Is.SameAs(category));
            Assert.That(category.CategoryId, Is.EqualTo("matematicas"));
            Assert.That(category.DisplayName, Is.EqualTo("Play and Learn"));
            Assert.That(catalog.GetGames(definition.Category), Does.Contain(definition));
            Assert.That(System.Array.Exists(EditorBuildSettings.scenes, scene => scene.enabled && scene.path == ScenePath), Is.True);
        }

        [Test]
        public void SceneProvidesTheConfiguredUGuiInputRouteAndFourCards()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                MemoramaGame game = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<MemoramaGame>(true))
                    .Single();
                GameSession session = new();
                game.Configure(new AppServices(session, new GameLauncher(session, new NoOpSceneLoader(), "Lobby")));
                Canvas.ForceUpdateCanvases();

                Transform[] hierarchy = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .ToArray();
                Assert.That(hierarchy.Count(item => item.name.StartsWith("Card")), Is.EqualTo(4));
                Assert.That(hierarchy.Any(item => item.name == "Exit"), Is.True);
                Assert.That(hierarchy.Any(item => item.name == "AnimalNameToast"), Is.True);
                Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<EventSystem>(true)).Count(), Is.EqualTo(1));
                Assert.That(scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<StandaloneInputModule>(true)).Count(), Is.EqualTo(1));

                Button[] cards = hierarchy
                    .Where(item => item.name.StartsWith("Card"))
                    .Select(item => item.GetComponent<Button>())
                    .Where(button => button != null)
                    .ToArray();
                Assert.That(cards, Has.Length.EqualTo(4));
                Assert.That(cards.All(button =>
                    button.transition == Selectable.Transition.None && button.targetGraphic == null), Is.True);
                Assert.That(cards.All(button => !button.interactable), Is.True);
                Assert.That(cards.All(button => button.transform.Find("Animal").gameObject.activeSelf), Is.True);
                Sprite stickerBlue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/Memorama/Art/StickerBlue.png");
                Sprite stickerPink = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/Memorama/Art/StickerPink.png");
                Sprite[] fronts = cards.Select(button => button.transform.Find("Face").GetComponent<Image>().sprite).ToArray();
                Assert.That(fronts.Count(front => front == stickerBlue), Is.EqualTo(2));
                Assert.That(fronts.Count(front => front == stickerPink), Is.EqualTo(2));

                Transform toast = hierarchy.Single(item => item.name == "AnimalNameToast");
                Transform fillTransform = toast.Find("Fill");
                Transform surfaceTransform = toast.Find("Surface");
                Assert.That(fillTransform, Is.Not.Null);
                Assert.That(surfaceTransform, Is.Not.Null);
                RoundedSurface toastFill = fillTransform.GetComponent<RoundedSurface>();
                RoundedSurface toastSurface = surfaceTransform.GetComponent<RoundedSurface>();
                Text toastLabel = toast.GetComponentInChildren<Text>(true);
                Font nunitoExtraBold = AssetDatabase.LoadAssetAtPath<Font>("Assets/App/Theme/Fonts/Nunito-ExtraBold.ttf");
                Assert.That(toastSurface, Is.Not.Null);
                Assert.That(toastFill, Is.Not.Null);
                Assert.That(toastFill.color.a, Is.EqualTo(1f));
                Assert.That(toastFill.OutlineThickness, Is.EqualTo(0f));
                Assert.That(toastFill.CornerRadius, Is.EqualTo(toastSurface.CornerRadius));
                Assert.That(toastSurface.color.a, Is.EqualTo(1f));
                Assert.That(toastLabel.font, Is.SameAs(nunitoExtraBold));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void SuppliedArtworkAndAudioAssetsAreImportedAndAssigned()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/Memorama/Art/Level1Back.png"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/Memorama/Art/Level1Front.png"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/Memorama/Art/Cow.png"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/Memorama/Art/Rabbit.png"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/Memorama/Art/Dog.png"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/Memorama/Art/Pig.png"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/Memorama/Audio/Cow.mp3"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/Memorama/Audio/Rabbit.mp3"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/Memorama/Audio/Dog.mp3"), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/Memorama/Audio/Pig.mp3"), Is.Not.Null);
            AudioClip initialInstruction = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/App/Games/Memorama/Audio/InitialInstruction.mp3");
            Assert.That(initialInstruction, Is.Not.Null);
            AudioClip successSfx = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Resources/ShapeAnalogy/SFX/sfx_success_true_answer.mp3");
            Assert.That(successSfx, Is.Not.Null);
            Sprite fourStar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Celebration/4Star.png");
            Sprite fiveStar = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/App/Games/ShapeAnalogy/Celebration/5star.png");
            Assert.That(fourStar, Is.Not.Null);
            Assert.That(fiveStar, Is.Not.Null);

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                MemoramaGame game = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<MemoramaGame>(true))
                    .Single();
                var serializedGame = new SerializedObject(game);
                Assert.That(serializedGame.FindProperty("cowArtwork").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedGame.FindProperty("cowNameAudio").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedGame.FindProperty("rabbitNameAudio").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedGame.FindProperty("dogNameAudio").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedGame.FindProperty("pigNameAudio").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedGame.FindProperty("cowFront").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedGame.FindProperty("rabbitFront").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedGame.FindProperty("dogFront").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedGame.FindProperty("pigFront").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedGame.FindProperty("initialInstruction").objectReferenceValue, Is.SameAs(initialInstruction));
                Assert.That(serializedGame.FindProperty("matchSuccessSfx").objectReferenceValue, Is.SameAs(successSfx));
                Assert.That(serializedGame.FindProperty("toastFont").objectReferenceValue,
                    Is.SameAs(AssetDatabase.LoadAssetAtPath<Font>("Assets/App/Theme/Fonts/Nunito-ExtraBold.ttf")));
                Assert.That(serializedGame.FindProperty("fourStarParticle").objectReferenceValue, Is.SameAs(fourStar));
                Assert.That(serializedGame.FindProperty("fiveStarParticle").objectReferenceValue, Is.SameAs(fiveStar));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void EnglishCompletionComplimentResourcesAreAvailable()
        {
            AudioClip[] compliments = Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Compliments/en");

            Assert.That(compliments, Is.Not.Empty);
            Assert.That(compliments.All(clip => clip != null), Is.True);
        }

        private sealed class NoOpSceneLoader : ISceneLoader
        {
            public void Load(string sceneName)
            {
            }
        }
    }
}
