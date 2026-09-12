using System.Collections;
using System.Collections.Generic;
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

namespace Lbs.MiniGames.Games.ShapePut
{
    public sealed class ShapePutGame : MonoBehaviour, IAppScene, ILevelTransitionParticipant
    {
        private static readonly Color Background = new(.9686f, .9608f, .9804f);
        private static readonly Color NormalBorder = new(.78f, .78f, .78f, 1f);
        private static readonly Color Error = new(.70f, .15f, .12f);
        private static readonly Color Success = new(.09f, .48f, .29f);

        [SerializeField] private Sprite principalArtwork;
        [SerializeField] private Sprite option1Artwork, option2Artwork, option3Artwork, option4Artwork;
        [SerializeField] private Sprite exitIcon, hongNeutral, hong1, hong2, hong3, finalStar;
        [SerializeField] private Sprite celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3;
        [SerializeField] private AudioClip instruction, successSfx, failSfx;
        [SerializeField] private AudioClip[] compliments, encouragements;
        [SerializeField] private SharedAudioLibrary sharedLibrary;
        [SerializeField] private Font font, scoreFont;
        [SerializeField] private FinalCelebrationConfiguration celebrationConfiguration;

        private readonly SelectionGameState state = new();
        private readonly List<Button> answers = new();
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

            board = new GameObject("ShapePutBoard", typeof(RectTransform)).GetComponent<RectTransform>();
            board.SetParent(canvas.transform, false);
            UiFactory.Stretch(board, 0);
            UiFactory.Stretch(UiFactory.CreateImage(board, "Background", Background).rectTransform, 0);

            Image principal = UiFactory.CreateImage(board, "Principal", Color.white);
            principal.sprite = principalArtwork;
            principal.preserveAspect = true;
            principal.raycastTarget = false;
            Pixel(principal.rectTransform, new Vector2(1030f, 410f), new Vector2(1120f, 710f));

            levelChrome = LevelChromeFactory.Build(board, font, exitIcon, hongNeutral, ReturnToLobby, ToggleInstruction);
            hongImage = levelChrome.HongImage;
            CreateOption(ShapePutRule.Option1, option1Artwork, new Vector2(470f, 900f));
            CreateOption(ShapePutRule.Option2, option2Artwork, new Vector2(865f, 900f));
            CreateOption(ShapePutRule.Option3, option3Artwork, new Vector2(1260f, 900f));
            CreateOption(ShapePutRule.Option4, option4Artwork, new Vector2(1655f, 900f));
            EnsureScoreFont();
        }

        private void CreateOption(string answerId, Sprite artwork, Vector2 center)
        {
            RoundedSurface surface = UiFactory.CreateRoundedSurface(board, answerId + "Card", NormalBorder, 26f);
            Pixel(surface.rectTransform, center, new Vector2(350f, 185f));
            surface.OutlineThickness = 5f;
            RoundedSurface fill = UiFactory.CreateRoundedSurface(surface.rectTransform, "CardFill", Color.white, 21f, false);
            UiFactory.Stretch(fill.rectTransform, surface.OutlineThickness);

            Image image = UiFactory.CreateImage(surface.rectTransform, "Artwork", Color.white);
            image.sprite = artwork;
            image.preserveAspect = true;
            image.raycastTarget = false;
            UiFactory.Stretch(image.rectTransform, 22f);

            Button button = surface.gameObject.AddComponent<Button>();
            button.targetGraphic = surface;
            button.onClick.AddListener(() => Select(answerId, surface));
            answers.Add(button);
        }

        private void Select(string answerId, RoundedSurface surface)
        {
            if (state.Phase != SelectionPhase.Ready) return;
            StopInstruction();
            SetInteractable(false);
            bool correct = state.Select(answerId, ShapePutRule.CorrectAnswer);
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
            selectionSequence = null;
        }

        private IEnumerator ResolveCorrect(RoundedSurface surface)
        {
            yield return CardAnimator.PunchPlace(surface.rectTransform);
            surface.color = Success;
            if (successSfx) audio?.PlaySfx(successSfx);
            PlayRandom(compliments);
            celebrationPresenter.ShowCelebration(board, CelebrationInput());
            yield return new WaitForSecondsRealtime(celebrationPresenter.PresentationDelay);
            celebrationPresenter.ShowFinal(CelebrationInput());
            services?.GameLauncher.Complete(new MiniGameResult(LevelSequenceRoute.ShapePutGameId, MiniGameCompletionState.Completed, state.Score, 1, 1, services.Session.SelectedDifficultyId));
            state.FinishCelebration();
            yield return new WaitForSecondsRealtime(2f);
            services?.LevelSequence?.Advance(LevelSequenceRoute.ShapePutSuccessTarget);
            selectionSequence = null;
        }

        private FinalCelebrationInput CelebrationInput() => new(state.Score, state.StarCount, scoreFont ? scoreFont : font, finalStar, celebration4Star, celebration5Star, circleConfetti, rectangularConfetti, serpentina, serpentina2, serpentina3);

        private void SetInteractable(bool value)
        {
            foreach (Button answer in answers) if (answer) answer.interactable = value;
            if (levelChrome == null) return;
            if (levelChrome.ExitButton != null) levelChrome.ExitButton.interactable = value;
            if (levelChrome.HongButton != null) levelChrome.HongButton.interactable = value;
        }

        private void EnsureScoreFont()
        {
            if (scoreFont) return;
            scoreFont = Resources.Load<Font>("Fonts/Nunito-Black");
            if (!scoreFont) scoreFont = Resources.Load<Font>("Nunito-Black");
            if (!scoreFont) scoreFont = font;
        }

        private void EnsureSharedAudio()
        {
            if (successSfx == null) successSfx = sharedLibrary != null ? sharedLibrary.SuccessClip : Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_success_true_answer");
            if (failSfx == null) failSfx = sharedLibrary != null ? sharedLibrary.FailClip : Resources.Load<AudioClip>("ShapeAnalogy/SFX/sfx_fail_incorrect_answer");
            if (compliments == null || compliments.Length == 0) compliments = sharedLibrary != null && sharedLibrary.Compliments.Count > 0 ? CopyClips(sharedLibrary.Compliments) : Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Compliments/en");
            if (encouragements == null || encouragements.Length == 0) encouragements = sharedLibrary != null && sharedLibrary.Encouragements.Count > 0 ? CopyClips(sharedLibrary.Encouragements) : Resources.LoadAll<AudioClip>("ShapeAnalogy/Voice/Encouragement/en");
        }

        private static AudioClip[] CopyClips(IReadOnlyList<AudioClip> clips)
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

        private static void Pixel(RectTransform rect, Vector2 topOriginCenter, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(topOriginCenter);
            rect.sizeDelta = size;
        }
    }
}
