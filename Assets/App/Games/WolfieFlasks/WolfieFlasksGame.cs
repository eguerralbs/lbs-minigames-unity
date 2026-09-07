using System.Collections;
using Lbs.MiniGames.Bootstrap;
using Lbs.MiniGames.GameKits.DragDrop;
using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Navigation;
using Lbs.MiniGames.Shared;
using Lbs.MiniGames.Shared.Audio;
using Lbs.MiniGames.Shared.Results;
using Lbs.MiniGames.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.WolfieFlasks
{
    public sealed class WolfieFlasksGame : MonoBehaviour, IAppScene, ILevelTransitionParticipant
    {
        private static readonly Color Background = new(0.9686f, 0.9608f, 0.9804f, 1f);
        private static readonly Color NormalBorder = new(.78f, .78f, .78f, 1f);
        private static readonly Color Error = new(.70f, .15f, .12f);
        private static readonly Color Success = new(.09f, .48f, .29f);

        [SerializeField] private Sprite principalSprite;
        [SerializeField] private Sprite option1Sprite;
        [SerializeField] private Sprite option2Sprite;
        [SerializeField] private Sprite exitIcon, hongNeutral, hong1, hong2, hong3, finalStar;
        [SerializeField] private Sprite celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3;
        [SerializeField] private AudioClip instruction, successSfx, failSfx;
        [SerializeField] private AudioClip[] compliments, encouragements;
        [SerializeField] private SharedAudioLibrary sharedLibrary;
        [SerializeField] private Font font, scoreFont;
        [SerializeField] private FinalCelebrationConfiguration celebrationConfiguration;

        private readonly SelectionGameState state = new();
        private readonly System.Collections.Generic.List<Button> answers = new();
        private AppServices services;
        private IAppAudioService audio;
        private RectTransform board;
        private Image hongImage;
        private LevelChrome levelChrome;
        private Coroutine hongPlayback;
        private Coroutine selectionSequence;
        private FinalCelebrationPresenter celebrationPresenter;
        private bool transitionHandoffPending;

        public RectTransform TransitionRoot => board;

        public void Configure(AppServices appServices)
        {
            services = appServices;
            audio = appServices?.Audio;
            celebrationPresenter ??= new FinalCelebrationPresenter(this, celebrationConfiguration);
            Build();
            EnsureSharedAudio();
            if (appServices?.LevelSequence?.IsTransitioning == true) transitionHandoffPending = true;
            else StartInstructionPlayback();
        }

        public void CompleteTransitionHandoff()
        {
            if (!transitionHandoffPending) return;
            transitionHandoffPending = false;
            StartInstructionPlayback();
        }

        private void StartInstructionPlayback()
        {
            PlayInstruction();
            if (hongPlayback == null) hongPlayback = StartCoroutine(AnimateHong());
        }

        private void Build()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (!canvas || board != null) return;

            board = new GameObject("WolfieFlasksBoard", typeof(RectTransform)).GetComponent<RectTransform>();
            board.SetParent(canvas.transform, false);
            UiFactory.Stretch(board, 0);
            UiFactory.Stretch(UiFactory.CreateImage(board, "Background", Background).rectTransform, 0);

            levelChrome = LevelChromeFactory.Build(board, font, exitIcon, hongNeutral, ReturnToLobby, ToggleInstruction);
            hongImage = levelChrome.HongImage;

            RectTransform principalRoot = new GameObject("PrincipalRoot", typeof(RectTransform)).GetComponent<RectTransform>();
            principalRoot.SetParent(board, false);
            Pixel(principalRoot, new Vector2(960, 360), new Vector2(700, 660));
            Image principal = UiFactory.CreateImage(principalRoot, "Principal", Color.white);
            principal.sprite = principalSprite;
            principal.preserveAspect = true;
            principal.raycastTarget = false;
            UiFactory.Stretch(principal.rectTransform, 0);

            CreateOption("option1", option1Sprite, new Vector2(600, 890));
            CreateOption("option2", option2Sprite, new Vector2(1320, 890));
            EnsureScoreFont();
        }

        private void EnsureScoreFont()
        {
            if (scoreFont) return;
            scoreFont = Resources.Load<Font>("Fonts/Nunito-Black");
            if (!scoreFont) scoreFont = Resources.Load<Font>("Nunito-Black");
            if (!scoreFont) scoreFont = font;
        }

        private void CreateOption(string id, Sprite artwork, Vector2 center)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, id + "Card", Color.white, 22f);
            Pixel(surface.rectTransform, center, new Vector2(640, 170));
            surface.OutlineThickness = 4f;
            surface.color = NormalBorder;
            RoundedSurface inner = UiFactory.CreateRoundedSurface(surface.rectTransform, "Fill", Color.white, 18f, false);
            UiFactory.Stretch(inner.rectTransform, surface.OutlineThickness);

            Image image = UiFactory.CreateImage(surface.rectTransform, "Artwork", Color.white);
            image.sprite = artwork;
            image.preserveAspect = true;
            image.raycastTarget = false;
            RectTransform imageRect = image.rectTransform;
            imageRect.anchorMin = imageRect.anchorMax = new Vector2(.5f, .5f);
            imageRect.pivot = new Vector2(.5f, .5f);
            imageRect.anchoredPosition = Vector2.zero;
            imageRect.sizeDelta = new Vector2(120, 140);

            surface.raycastTarget = true;
            inner.raycastTarget = false;
            Button button = surface.gameObject.AddComponent<Button>();
            button.targetGraphic = surface;
            button.onClick.AddListener(() => Select(id, surface));
            answers.Add(button);
        }

        private void Select(string id, RoundedSurface surface)
        {
            if (state.Phase != SelectionPhase.Ready) return;
            StopInstruction();
            SetInteractable(false);
            bool correct = state.Select(id, WolfieFlasksRule.CorrectAnswer);
            selectionSequence = StartCoroutine(correct ? ResolveCorrect(surface) : ResolveIncorrect(surface));
        }

        private IEnumerator ResolveIncorrect(RoundedSurface surface)
        {
            surface.color = Error;
            if (failSfx) audio?.PlaySfx(failSfx);
            PlayRandom(encouragements);
            yield return CardAnimator.ShakeBoard(board);
            surface.color = NormalBorder;
            state.FinishIncorrect();
            SetInteractable(true);
        }

        private IEnumerator ResolveCorrect(RoundedSurface surface)
        {
            yield return CardAnimator.PunchPlace(surface.rectTransform);
            surface.color = Success;
            if (successSfx) audio?.PlaySfx(successSfx);
            PlayRandom(compliments);
            CreateCelebration();
            yield return new WaitForSecondsRealtime(celebrationPresenter.PresentationDelay);
            CreateFinal();
            state.FinishCelebration();
            yield return new WaitForSecondsRealtime(2f);
            services?.LevelSequence?.Advance(LevelSequenceRoute.WolfieFlasksSuccessTarget);
            state.EnableFinalInput();
            selectionSequence = null;
        }

        private FinalCelebrationInput CelebrationInput() => new(state.Score, state.StarCount, scoreFont ? scoreFont : font, finalStar, celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3);
        private void CreateCelebration() => celebrationPresenter.ShowCelebration(board, CelebrationInput());
        private void CreateFinal()
        {
            celebrationPresenter.ShowFinal(CelebrationInput());
            services?.GameLauncher.Complete(new MiniGameResult("wolfie.flasks", MiniGameCompletionState.Completed, state.Score, 1, 1, services.Session.SelectedDifficultyId));
        }

        private void SetInteractable(bool value)
        {
            foreach (Button answer in answers) if (answer) answer.interactable = value;
            if (levelChrome == null) return;
            if (levelChrome.ExitButton != null) levelChrome.ExitButton.interactable = value;
            if (levelChrome.HongButton != null) levelChrome.HongButton.interactable = value;
        }

        private void EnsureSharedAudio()
        {
            if (successSfx == null) successSfx = sharedLibrary != null ? sharedLibrary.SuccessClip : Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_success_true_answer");
            if (failSfx == null) failSfx = sharedLibrary != null ? sharedLibrary.FailClip : Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_fail_incorrect_answer");
            if (compliments == null || compliments.Length == 0) compliments = sharedLibrary != null && sharedLibrary.Compliments.Count > 0 ? CopyClips(sharedLibrary.Compliments) : Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Compliments/en");
            if (encouragements == null || encouragements.Length == 0) encouragements = sharedLibrary != null && sharedLibrary.Encouragements.Count > 0 ? CopyClips(sharedLibrary.Encouragements) : Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Encouragement/en");
        }

        private static AudioClip[] CopyClips(System.Collections.Generic.IReadOnlyList<AudioClip> clips)
        {
            AudioClip[] copy = new AudioClip[clips.Count];
            for (int index = 0; index < copy.Length; index++) copy[index] = clips[index];
            return copy;
        }

        private void PlayInstruction() { if (instruction) audio?.PlayVoice(instruction); }
        private void StopInstruction() { audio?.StopVoiceIfPlaying(instruction); }
        private void ToggleInstruction() { if (state.Phase != SelectionPhase.Ready) return; if (audio != null && audio.IsVoicePlaying(instruction)) audio.StopVoiceIfPlaying(instruction); else PlayInstruction(); }
        private void PlayRandom(AudioClip[] clips) { if (clips != null && clips.Length > 0) audio?.PlayVoice(clips[Random.Range(0, clips.Length)]); }
        private IEnumerator AnimateHong() { int[] frames = { 1, 2, 3, 2, 1 }; int index = 0; while (true) { bool playing = audio != null && audio.IsVoicePlaying(instruction); if (hongImage) hongImage.sprite = playing ? (frames[index++ % frames.Length] == 1 ? hong1 : frames[(index - 1 + frames.Length) % frames.Length] == 2 ? hong2 : hong3) : hongNeutral; yield return new WaitForSecondsRealtime(.18f); } }
        private void Update() { if (state.AcceptFinalInput() && (Input.GetMouseButtonDown(0) || Input.touchCount > 0)) ReturnToLobby(); }
        private void ReturnToLobby() { if (state.Phase == SelectionPhase.ResolvingIncorrect || state.Phase == SelectionPhase.Celebrating || (state.Phase == SelectionPhase.Final && !state.IsFinalInputEnabled)) return; audio?.StopMusic(); services?.GameLauncher.ShowLobby(); }
        private void OnDisable()
        {
            if (selectionSequence != null) StopCoroutine(selectionSequence);
            if (hongPlayback != null) StopCoroutine(hongPlayback);
            selectionSequence = null;
            hongPlayback = null;
            celebrationPresenter?.Clear();
            audio?.StopVoiceIfPlaying(instruction);
        }

        private static void Pixel(RectTransform rect, Vector2 top, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(top);
            rect.sizeDelta = size;
        }
    }
}
