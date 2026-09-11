using System.Collections;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using Lbs.MiniGames.Shared.Audio;
using Lbs.MiniGames.Shared.Results;
using Lbs.MiniGames.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.Memorama
{
    public sealed class MemoramaGame : MonoBehaviour, IAppScene
    {
        public const string StableGameId = "juega-aprende.memoria-animales";

        private static readonly string[] Level1AnimalIds = { "cow", "pig", "cow", "pig" };
        private static readonly Vector2[] Level1CardCenters =
        {
            new(735f, 390f), new(1185f, 390f), new(735f, 790f), new(1185f, 790f)
        };
        private static readonly string[] Level2AnimalIds = { "rabbit", "dog", "pig", "rabbit", "dog", "pig" };
        private static readonly Vector2[] Level2CardCenters =
        {
            new(510f, 390f), new(960f, 390f), new(1410f, 390f),
            new(510f, 790f), new(960f, 790f), new(1410f, 790f)
        };
        private const float OpeningPreviewDuration = 0.5f;
        private const float OpeningFlipDuration = 0.32f;
        private const float CardFlipDuration = 0.32f;
        private const float OpeningShuffleDuration = 1.5f;
        private const int OpeningShuffleSwapCount = 10;
        private const float ShuffleSwapDuration = OpeningShuffleDuration / OpeningShuffleSwapCount;
        private const float ShuffleTiltDegrees = 7f;
        private const float LevelTransitionDuration = 0.6f;
        private const float LevelCompletionDelay = 2f;
        private const float ReferenceWidth = 1920f;
        private const string ComplimentsResourcePath = "ShapeAnalogy/Voice/Compliments/en";
        private const float CompletionVoiceWaitPadding = 0.5f;
        private const int ToastFontSize = 56;
        private const float ToastMinimumWidth = 200f;
        private const float ToastHorizontalPadding = 96f;
        private const float ToastHeight = 112f;

        [SerializeField] private Sprite background;
        [SerializeField] private Sprite cardBack;
        [SerializeField] private Sprite level2Back;
        [SerializeField] private Sprite cardFront;
        [SerializeField] private Sprite cowFront;
        [SerializeField] private Sprite rabbitFront;
        [SerializeField] private Sprite dogFront;
        [SerializeField] private Sprite pigFront;
        [SerializeField] private Sprite cowArtwork;
        [SerializeField] private Sprite rabbitArtwork;
        [SerializeField] private Sprite dogArtwork;
        [SerializeField] private Sprite pigArtwork;
        [SerializeField] private Sprite exitIcon;
        [SerializeField] private AudioClip cowNameAudio;
        [SerializeField] private AudioClip rabbitNameAudio;
        [SerializeField] private AudioClip dogNameAudio;
        [SerializeField] private AudioClip pigNameAudio;
        [SerializeField] private AudioClip initialInstruction;
        [SerializeField] private AudioClip matchSuccessSfx;
        [SerializeField] private AudioClip[] compliments;
        [SerializeField] private Font font;
        [SerializeField] private Font toastFont;
        [SerializeField] private Sprite fourStarParticle;
        [SerializeField] private Sprite fiveStarParticle;

        private CardView[] cardViews = new CardView[0];
        private Coroutine[] cardFlipAnimations = new Coroutine[0];
        private AppServices services;
        private IAppAudioService audio;
        private MemoramaBoard board;
        private bool mismatchHiding;
        private RectTransform boardRoot;
        private RectTransform levelRoot;
        private Vector2[] cardCenters;
        private int currentLevelIndex;
        private GameObject nameToast;
        private Text nameToastLabel;
        private Coroutine mismatchResolution;
        private Coroutine toastPlayback;
        private Coroutine openingSequence;
        private Coroutine completionParticlesCleanup;
        private Coroutine completionComplimentPlayback;
        private Coroutine levelTransition;
        private FinalCelebrationParticles completionParticles;
        private bool openingActive;
        private bool openingCardsVisible;

        public MemoramaPhase Phase => board?.Phase ?? MemoramaPhase.Ready;
        public bool IsCompleted => board?.Phase == MemoramaPhase.Complete;

        public void Configure(AppServices appServices)
        {
            StopLevelTransition();
            StopMismatchResolution();
            StopCardFlipAnimations();
            StopOpeningSequence();
            ClearCompletionParticles();
            StopCompletionComplimentPlayback();
            RestoreCardTransforms();
            ClearCurrentLevel();
            services = appServices;
            audio = appServices?.Audio;
            EnsureCompliments();
            Build();
            if (boardRoot == null) return;
            BuildLevel(0);
            StartOpeningSequence();
        }

        private void Build()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null || boardRoot != null) return;

            boardRoot = new GameObject("MemoramaBoard", typeof(RectTransform)).GetComponent<RectTransform>();
            boardRoot.SetParent(canvas.transform, false);
            UiFactory.Stretch(boardRoot, 0f);

            Image backgroundImage = UiFactory.CreateImage(boardRoot, "Background", Color.white);
            backgroundImage.sprite = background;
            backgroundImage.preserveAspect = false;
            backgroundImage.raycastTarget = false;
            UiFactory.Stretch(backgroundImage.rectTransform, 0f);

            LevelChrome chrome = LevelChromeFactory.Build(boardRoot, font, exitIcon, null, ReturnToLobby, null);
            chrome.HongButton.gameObject.SetActive(false);

            CreateNameToast();
        }

        private void BuildLevel(int levelIndex)
        {
            currentLevelIndex = levelIndex;
            string[] animalIds = ShuffleAnimalIds(levelIndex == 0 ? Level1AnimalIds : Level2AnimalIds);
            cardCenters = levelIndex == 0 ? Level1CardCenters : Level2CardCenters;
            board = new MemoramaBoard(animalIds);
            cardViews = new CardView[board.CardCount];
            cardFlipAnimations = new Coroutine[board.CardCount];
            Sprite back = levelIndex == 0 ? cardBack : level2Back != null ? level2Back : cardBack;

            levelRoot = new GameObject($"Level{levelIndex + 1}Board", typeof(RectTransform)).GetComponent<RectTransform>();
            levelRoot.SetParent(boardRoot, false);
            UiFactory.Stretch(levelRoot, 0f);
            for (int index = 0; index < cardViews.Length; index++) CreateCard(index, back);
            RefreshAllCards();
        }

        private static string[] ShuffleAnimalIds(string[] source)
        {
            string[] shuffled = (string[])source.Clone();
            for (int index = shuffled.Length - 1; index > 0; index--)
            {
                int swapIndex = Random.Range(0, index + 1);
                (shuffled[index], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[index]);
            }

            return shuffled;
        }

        private void StartOpeningSequence()
        {
            openingActive = true;
            openingCardsVisible = true;
            RefreshAllCards();
            SetCardsInteractable(false);
            openingSequence = StartCoroutine(PlayOpeningSequence());
        }

        private void CreateNameToast()
        {
            nameToast = new GameObject("AnimalNameToast", typeof(RectTransform));
            nameToast.transform.SetParent(boardRoot, false);
            RectTransform toastRect = nameToast.GetComponent<RectTransform>();
            Pixel(toastRect, new Vector2(960f, 135f), new Vector2(ToastMinimumWidth, ToastHeight));

            RoundedSurface fill = UiFactory.CreateRoundedSurface(toastRect, "Fill", Color.white, 34f, false);
            UiFactory.Stretch(fill.rectTransform, 0f);

            RoundedSurface surface = UiFactory.CreateRoundedSurface(toastRect, "Surface", Color.white, 34f, false);
            surface.OutlineThickness = 5f;
            UiFactory.Stretch(surface.rectTransform, 0f);
            nameToastLabel = UiFactory.CreateText(toastRect, "Label", toastFont, ToastFontSize, TextAnchor.MiddleCenter, new Color(0.141f, 0.102f, 0.208f));
            nameToastLabel.raycastTarget = false;
            nameToastLabel.resizeTextForBestFit = false;
            nameToastLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            UiFactory.Stretch(nameToastLabel.rectTransform, 18f);
            nameToast.SetActive(false);
        }

        private void CreateCard(int index, Sprite back)
        {
            Button button = UiFactory.CreateButton(levelRoot, $"Card{index + 1}", font, string.Empty, Color.clear);
            RectTransform cardRect = button.GetComponent<RectTransform>();
            Pixel(cardRect, cardCenters[index], new Vector2(350f, 350f));

            Image face = UiFactory.CreateImage(cardRect, "Face", Color.white);
            face.preserveAspect = true;
            face.raycastTarget = false;
            UiFactory.Stretch(face.rectTransform, 0f);

            string animalId = board.GetAnimalId(index);
            Image animal = UiFactory.CreateImage(cardRect, "Animal", Color.white);
            animal.sprite = AnimalFor(animalId);
            animal.preserveAspect = true;
            animal.raycastTarget = false;
            UiFactory.Stretch(animal.rectTransform, 48f);

            int capturedIndex = index;
            button.transition = Selectable.Transition.None;
            button.targetGraphic = null;
            button.onClick.AddListener(() => SelectCard(capturedIndex));
            cardViews[index] = new CardView(button, cardRect, face, animal, FrontFor(animalId), back);
        }

        private void SelectCard(int index)
        {
            if (openingActive) return;
            MemoramaSelectionResult result = board.Select(index);
            if (result == MemoramaSelectionResult.Ignored) return;

            StartCardFlip(index, true);
            audio?.PlayVoiceOneShot(NameAudioFor(board.GetAnimalId(index)));

            if (result == MemoramaSelectionResult.PairMismatched)
            {
                SetCardsInteractable(false);
                mismatchResolution = StartCoroutine(ResolveMismatchAfterDelay());
                return;
            }

            if (result == MemoramaSelectionResult.PairMatched)
            {
                RefreshAllCards();
                audio?.PlaySfx(matchSuccessSfx);
                ShowAnimalName(board.GetAnimalId(index));
                if (board.Phase == MemoramaPhase.Complete)
                {
                    ShowCompletionParticles();
                    PlayCompletionComplimentAfterFinalName(NameAudioFor(board.GetAnimalId(index)));
                    if (currentLevelIndex == 0) levelTransition = StartCoroutine(TransitionToNextLevel());
                }
            }
        }

        private void EnsureCompliments()
        {
            if (compliments == null || compliments.Length == 0)
            {
                compliments = Resources.LoadAll<AudioClip>(ComplimentsResourcePath);
            }
        }

        private void PlayCompletionComplimentAfterFinalName(AudioClip finalNameAudio)
        {
            if (audio == null || compliments == null || compliments.Length == 0) return;

            AudioClip compliment = compliments[Random.Range(0, compliments.Length)];
            if (compliment == null) return;

            StopCompletionComplimentPlayback();
            completionComplimentPlayback = StartCoroutine(WaitForFinalNameThenPlayCompliment(compliment, finalNameAudio));
        }

        private IEnumerator WaitForFinalNameThenPlayCompliment(AudioClip compliment, AudioClip finalNameAudio)
        {
            float timeout = Time.realtimeSinceStartup + Mathf.Max(1f, (finalNameAudio ? finalNameAudio.length : 0f) + CompletionVoiceWaitPadding);
            while (audio != null && audio.IsVoicePlaying() && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            if (audio != null) audio.PlayVoice(compliment);
            completionComplimentPlayback = null;
        }

        private void StopCompletionComplimentPlayback()
        {
            if (completionComplimentPlayback != null) StopCoroutine(completionComplimentPlayback);
            completionComplimentPlayback = null;
        }

        private void ShowCompletionParticles()
        {
            ClearCompletionParticles();
            RectTransform particlesRoot = new GameObject("CompletionStars", typeof(RectTransform)).GetComponent<RectTransform>();
            particlesRoot.SetParent(levelRoot, false);
            UiFactory.Stretch(particlesRoot, 0f);
            particlesRoot.SetAsLastSibling();
            completionParticles = particlesRoot.gameObject.AddComponent<FinalCelebrationParticles>();
            completionParticles.Initialize(fourStarParticle, fiveStarParticle, null, null, null);
            completionParticlesCleanup = StartCoroutine(ClearCompletionParticlesAfterDuration(completionParticles));
        }

        private IEnumerator ClearCompletionParticlesAfterDuration(FinalCelebrationParticles particles)
        {
            yield return new WaitForSecondsRealtime(FinalCelebrationParticles.Duration);
            if (completionParticles == particles) ClearCompletionParticles();
        }

        private void ClearCompletionParticles()
        {
            if (completionParticlesCleanup != null) StopCoroutine(completionParticlesCleanup);
            completionParticlesCleanup = null;
            if (completionParticles == null) return;

            completionParticles.StopAndClear();
            GameObject particlesObject = completionParticles.gameObject;
            completionParticles = null;
            if (Application.isPlaying) Destroy(particlesObject);
            else DestroyImmediate(particlesObject);
        }

        private IEnumerator ResolveMismatchAfterDelay()
        {
            yield return new WaitForSecondsRealtime(1f);
            int firstSelection = board.FirstSelection;
            int secondSelection = board.SecondSelection;
            board.ResolveMismatch();
            mismatchHiding = true;
            StartCardFlip(firstSelection, false);
            StartCardFlip(secondSelection, false);
            RefreshAllCards();

            while (cardFlipAnimations[firstSelection] != null || cardFlipAnimations[secondSelection] != null) yield return null;

            mismatchHiding = false;
            RefreshAllCards();
            mismatchResolution = null;
        }

        private void RefreshAllCards()
        {
            for (int index = 0; index < cardViews.Length; index++) RefreshCard(index);
        }

        private void RefreshCard(int index)
        {
            CardView view = cardViews[index];
            if (view == null) return;
            MemoramaCardState state = board.GetCardState(index);
            bool visible = openingCardsVisible || state != MemoramaCardState.Hidden;
            bool isAnimating = cardFlipAnimations[index] != null;
            if (!isAnimating) SetCardVisible(view, visible);
            view.Button.interactable = !isAnimating && !mismatchHiding && !openingActive && board.Phase == MemoramaPhase.Ready && state == MemoramaCardState.Hidden;
        }

        private void StartCardFlip(int index, bool reveal)
        {
            CardView view = cardViews[index];
            if (view == null)
            {
                RefreshCard(index);
                return;
            }

            if (cardFlipAnimations[index] != null) return;
            view.Rect.localRotation = Quaternion.identity;
            view.Button.interactable = false;
            cardFlipAnimations[index] = StartCoroutine(AnimateCardFlip(index, reveal));
        }

        private IEnumerator AnimateCardFlip(int index, bool reveal)
        {
            CardView view = cardViews[index];
            float elapsed = 0f;
            bool appearanceChanged = false;

            while (elapsed < CardFlipDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / CardFlipDuration);
                float rotation = progress <= 0.5f
                    ? Mathf.Lerp(0f, 90f, progress * 2f)
                    : Mathf.Lerp(90f, 0f, (progress - 0.5f) * 2f);
                view.Rect.localRotation = Quaternion.Euler(0f, rotation, 0f);

                if (!appearanceChanged && progress >= 0.5f)
                {
                    appearanceChanged = true;
                    SetCardVisible(view, reveal);
                }

                yield return null;
            }

            if (!appearanceChanged) SetCardVisible(view, reveal);
            view.Rect.localRotation = Quaternion.identity;
            cardFlipAnimations[index] = null;
            RefreshCard(index);
        }

        private void StopCardFlipAnimations(bool restorePresentation = true)
        {
            for (int index = 0; index < cardFlipAnimations.Length; index++)
            {
                if (cardFlipAnimations[index] != null) StopCoroutine(cardFlipAnimations[index]);
                cardFlipAnimations[index] = null;
            }

            if (!restorePresentation) return;
            RestoreCardRotations();
            if (board != null) RefreshAllCards();
        }

        private void StopMismatchResolution()
        {
            if (mismatchResolution != null) StopCoroutine(mismatchResolution);
            mismatchResolution = null;
            mismatchHiding = false;
        }

        private void SetCardVisible(CardView view, bool visible)
        {
            view.Face.sprite = visible ? view.Front : view.Back;
            view.Face.color = Color.white;
            view.Animal.gameObject.SetActive(visible);
        }

        private void SetCardsInteractable(bool interactable)
        {
            foreach (CardView view in cardViews) if (view != null) view.Button.interactable = interactable;
        }

        private IEnumerator PlayOpeningSequence()
        {
            RefreshAllCards();
            SetCardsInteractable(false);
            audio?.PlayVoice(initialInstruction);

            yield return new WaitForSecondsRealtime(OpeningPreviewDuration);
            yield return AnimateOpeningFlip();

            for (int swapIndex = 0; swapIndex < OpeningShuffleSwapCount; swapIndex++)
            {
                int firstIndex = Random.Range(0, cardViews.Length);
                int secondIndex = Random.Range(0, cardViews.Length - 1);
                if (secondIndex >= firstIndex) secondIndex++;
                yield return AnimateCardSwap(firstIndex, secondIndex);
            }

            audio?.StopVoiceIfPlaying(initialInstruction);
            openingActive = false;
            RefreshAllCards();
            openingSequence = null;
        }

        private IEnumerator AnimateOpeningFlip()
        {
            float elapsed = 0f;
            bool cardsHidden = false;

            while (elapsed < OpeningFlipDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / OpeningFlipDuration);
                float rotation = progress <= 0.5f
                    ? Mathf.Lerp(0f, 90f, progress * 2f)
                    : Mathf.Lerp(90f, 0f, (progress - 0.5f) * 2f);
                foreach (CardView view in cardViews)
                {
                    if (view != null) view.Rect.localRotation = Quaternion.Euler(0f, rotation, 0f);
                }

                if (!cardsHidden && progress >= 0.5f)
                {
                    cardsHidden = true;
                    openingCardsVisible = false;
                    RefreshAllCards();
                    SetCardsInteractable(false);
                }

                yield return null;
            }

            if (!cardsHidden)
            {
                openingCardsVisible = false;
                RefreshAllCards();
                SetCardsInteractable(false);
            }

            RestoreCardRotations();
        }

        private IEnumerator AnimateCardSwap(int firstIndex, int secondIndex)
        {
            CardView first = cardViews[firstIndex];
            CardView second = cardViews[secondIndex];
            Vector2 firstStart = first.Rect.anchoredPosition;
            Vector2 secondStart = second.Rect.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < ShuffleSwapDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / ShuffleSwapDuration);
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
                float tilt = Mathf.Sin(progress * Mathf.PI) * ShuffleTiltDegrees;
                first.Rect.anchoredPosition = Vector2.LerpUnclamped(firstStart, secondStart, easedProgress);
                second.Rect.anchoredPosition = Vector2.LerpUnclamped(secondStart, firstStart, easedProgress);
                first.Rect.localRotation = Quaternion.Euler(0f, 0f, tilt);
                second.Rect.localRotation = Quaternion.Euler(0f, 0f, -tilt);
                yield return null;
            }

            first.Rect.anchoredPosition = secondStart;
            second.Rect.anchoredPosition = firstStart;
            first.Rect.localRotation = Quaternion.identity;
            second.Rect.localRotation = Quaternion.identity;
        }

        private void StopOpeningSequence()
        {
            if (openingSequence != null) StopCoroutine(openingSequence);
            openingSequence = null;
            openingActive = false;
            openingCardsVisible = false;
            audio?.StopVoiceIfPlaying(initialInstruction);
        }

        private IEnumerator TransitionToNextLevel()
        {
            yield return new WaitForSecondsRealtime(LevelCompletionDelay);

            StopMismatchResolution();
            StopCardFlipAnimations();
            StopOpeningSequence();
            ClearCompletionParticles();

            RectTransform outgoingLevel = levelRoot;
            openingCardsVisible = true;
            BuildLevel(1);
            RectTransform incomingLevel = levelRoot;
            incomingLevel.anchoredPosition = new Vector2(ReferenceWidth, 0f);
            SetCardsInteractable(false);

            float elapsed = 0f;
            while (elapsed < LevelTransitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / LevelTransitionDuration));
                outgoingLevel.anchoredPosition = new Vector2(Mathf.Lerp(0f, -ReferenceWidth, progress), 0f);
                incomingLevel.anchoredPosition = new Vector2(Mathf.Lerp(ReferenceWidth, 0f, progress), 0f);
                yield return null;
            }

            incomingLevel.anchoredPosition = Vector2.zero;
            if (Application.isPlaying) Destroy(outgoingLevel.gameObject);
            else DestroyImmediate(outgoingLevel.gameObject);
            StartOpeningSequence();
            levelTransition = null;
        }

        private void StopLevelTransition()
        {
            if (levelTransition != null) StopCoroutine(levelTransition);
            levelTransition = null;
        }

        private void ClearCurrentLevel()
        {
            if (levelRoot != null)
            {
                if (Application.isPlaying) Destroy(levelRoot.gameObject);
                else DestroyImmediate(levelRoot.gameObject);
            }

            levelRoot = null;
            cardViews = new CardView[0];
            cardFlipAnimations = new Coroutine[0];
            cardCenters = null;
            board = null;
        }

        private void RestoreCardTransforms()
        {
            for (int index = 0; index < cardViews.Length; index++)
            {
                CardView view = cardViews[index];
                if (view == null) continue;
                view.Rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(cardCenters[index]);
            }

            RestoreCardRotations();
        }

        private void RestoreCardRotations()
        {
            foreach (CardView view in cardViews)
            {
                if (view != null) view.Rect.localRotation = Quaternion.identity;
            }
        }

        private void ShowAnimalName(string animalId)
        {
            if (toastPlayback != null) StopCoroutine(toastPlayback);
            nameToastLabel.text = animalId switch
            {
                "cow" => "Cow",
                "rabbit" => "Rabbit",
                "dog" => "Dog",
                _ => "Pig"
            };
            nameToast.SetActive(true);
            ResizeNameToast();
            toastPlayback = StartCoroutine(HideNameToast());
        }

        private void ResizeNameToast()
        {
            RectTransform toastRect = nameToast.GetComponent<RectTransform>();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(toastRect);
            float width = Mathf.Max(ToastMinimumWidth, nameToastLabel.preferredWidth + ToastHorizontalPadding);
            toastRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        private IEnumerator HideNameToast()
        {
            yield return new WaitForSecondsRealtime(1.5f);
            nameToast.SetActive(false);
            toastPlayback = null;
        }

        private Sprite AnimalFor(string animalId) => animalId switch
        {
            "cow" => cowArtwork,
            "rabbit" => rabbitArtwork,
            "dog" => dogArtwork,
            _ => pigArtwork
        };

        private Sprite FrontFor(string animalId) => animalId switch
        {
            "cow" => cowFront != null ? cowFront : cardFront,
            "rabbit" => rabbitFront != null ? rabbitFront : cardFront,
            "dog" => dogFront != null ? dogFront : cardFront,
            _ => pigFront != null ? pigFront : cardFront
        };

        private AudioClip NameAudioFor(string animalId) => animalId switch
        {
            "cow" => cowNameAudio,
            "rabbit" => rabbitNameAudio,
            "dog" => dogNameAudio,
            _ => pigNameAudio
        };

        private void ReturnToLobby()
        {
            if (boardRoot != null) boardRoot.gameObject.SetActive(false);
            StopLevelTransition();
            StopMismatchResolution();
            StopCardFlipAnimations(false);
            StopOpeningSequence();
            ClearCompletionParticles();
            StopCompletionComplimentPlayback();
            audio?.StopVoice();
            services?.GameLauncher.ShowLobby();
        }

        private void OnDisable()
        {
            StopLevelTransition();
            StopMismatchResolution();
            StopCardFlipAnimations(false);
            StopOpeningSequence();
            ClearCompletionParticles();
            StopCompletionComplimentPlayback();
            if (toastPlayback != null) StopCoroutine(toastPlayback);
            toastPlayback = null;
            audio?.StopVoice();
        }

        private static void Pixel(RectTransform rect, Vector2 topOriginCenter, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(topOriginCenter);
            rect.sizeDelta = size;
        }

        private sealed class CardView
        {
            public CardView(Button button, RectTransform rect, Image face, Image animal, Sprite front, Sprite back)
            {
                Button = button;
                Rect = rect;
                Face = face;
                Animal = animal;
                Front = front;
                Back = back;
            }

            public Button Button { get; }
            public RectTransform Rect { get; }
            public Image Face { get; }
            public Image Animal { get; }
            public Sprite Front { get; }
            public Sprite Back { get; }
        }
    }
}
