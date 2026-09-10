using System;
using System.Collections.Generic;

namespace Lbs.MiniGames.Games.Memorama
{
    public enum MemoramaCardState
    {
        Hidden,
        Revealed,
        Matched
    }

    public enum MemoramaPhase
    {
        Ready,
        ResolvingMismatch,
        Complete
    }

    public enum MemoramaSelectionResult
    {
        Ignored,
        FirstCardRevealed,
        PairMatched,
        PairMismatched
    }

    public sealed class MemoramaBoard
    {
        private readonly string[] animalIds;
        private readonly MemoramaCardState[] cardStates;
        private int firstSelection = -1;
        private int secondSelection = -1;
        private int matchedPairs;

        public MemoramaBoard(IReadOnlyList<string> animalIds)
        {
            if (animalIds == null || animalIds.Count == 0 || animalIds.Count % 2 != 0)
            {
                throw new ArgumentException("The board requires an even, non-empty number of cards.", nameof(animalIds));
            }

            this.animalIds = new string[animalIds.Count];
            cardStates = new MemoramaCardState[animalIds.Count];
            var occurrences = new Dictionary<string, int>();
            for (int index = 0; index < animalIds.Count; index++)
            {
                string animalId = animalIds[index];
                if (string.IsNullOrWhiteSpace(animalId)) throw new ArgumentException("Each card requires an animal id.", nameof(animalIds));
                this.animalIds[index] = animalId;
                occurrences.TryGetValue(animalId, out int count);
                occurrences[animalId] = count + 1;
            }

            foreach (int count in occurrences.Values)
            {
                if (count != 2) throw new ArgumentException("Each animal id must occur exactly twice.", nameof(animalIds));
            }
        }

        public MemoramaPhase Phase { get; private set; } = MemoramaPhase.Ready;
        public int FirstSelection => firstSelection;
        public int SecondSelection => secondSelection;
        public int MatchedPairs => matchedPairs;
        public int CardCount => cardStates.Length;

        public string GetAnimalId(int index) => animalIds[ValidateIndex(index)];
        public MemoramaCardState GetCardState(int index) => cardStates[ValidateIndex(index)];

        public MemoramaSelectionResult Select(int index)
        {
            ValidateIndex(index);
            if (Phase != MemoramaPhase.Ready || cardStates[index] != MemoramaCardState.Hidden) return MemoramaSelectionResult.Ignored;

            cardStates[index] = MemoramaCardState.Revealed;
            if (firstSelection < 0)
            {
                firstSelection = index;
                return MemoramaSelectionResult.FirstCardRevealed;
            }

            secondSelection = index;
            if (animalIds[firstSelection] == animalIds[secondSelection])
            {
                cardStates[firstSelection] = MemoramaCardState.Matched;
                cardStates[secondSelection] = MemoramaCardState.Matched;
                matchedPairs++;
                firstSelection = -1;
                secondSelection = -1;
                if (matchedPairs * 2 == cardStates.Length) Phase = MemoramaPhase.Complete;
                return MemoramaSelectionResult.PairMatched;
            }

            Phase = MemoramaPhase.ResolvingMismatch;
            return MemoramaSelectionResult.PairMismatched;
        }

        public void ResolveMismatch()
        {
            if (Phase != MemoramaPhase.ResolvingMismatch) return;
            cardStates[firstSelection] = MemoramaCardState.Hidden;
            cardStates[secondSelection] = MemoramaCardState.Hidden;
            firstSelection = -1;
            secondSelection = -1;
            Phase = MemoramaPhase.Ready;
        }

        private int ValidateIndex(int index)
        {
            if (index < 0 || index >= cardStates.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return index;
        }
    }
}
