using Lbs.MiniGames.Games.Memorama;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class MemoramaBoardTests
    {
        [Test]
        public void MatchingPair_RemainsRevealedAndReportsItsPair()
        {
            var board = CreateBoard();

            Assert.AreEqual(MemoramaSelectionResult.FirstCardRevealed, board.Select(0));
            Assert.AreEqual(MemoramaSelectionResult.PairMatched, board.Select(2));
            Assert.AreEqual(MemoramaCardState.Matched, board.GetCardState(0));
            Assert.AreEqual(MemoramaCardState.Matched, board.GetCardState(2));
            Assert.AreEqual(1, board.MatchedPairs);
            Assert.AreEqual(MemoramaPhase.Ready, board.Phase);
        }

        [Test]
        public void MismatchedPair_LocksSelectionUntilItIsResolved()
        {
            var board = CreateBoard();

            board.Select(0);
            Assert.AreEqual(MemoramaSelectionResult.PairMismatched, board.Select(1));
            Assert.AreEqual(MemoramaPhase.ResolvingMismatch, board.Phase);
            Assert.AreEqual(MemoramaSelectionResult.Ignored, board.Select(2));

            board.ResolveMismatch();
            Assert.AreEqual(MemoramaCardState.Hidden, board.GetCardState(0));
            Assert.AreEqual(MemoramaCardState.Hidden, board.GetCardState(1));
            Assert.AreEqual(MemoramaPhase.Ready, board.Phase);
        }

        [Test]
        public void FinalMatch_LeavesTheCompletedBoardVisible()
        {
            var board = CreateBoard();

            board.Select(0);
            board.Select(2);
            board.Select(1);
            board.Select(3);

            Assert.AreEqual(MemoramaPhase.Complete, board.Phase);
            Assert.AreEqual(MemoramaCardState.Matched, board.GetCardState(0));
            Assert.AreEqual(MemoramaCardState.Matched, board.GetCardState(1));
            Assert.AreEqual(MemoramaCardState.Matched, board.GetCardState(2));
            Assert.AreEqual(MemoramaCardState.Matched, board.GetCardState(3));
        }

        [Test]
        public void ThreePairBoard_CompletesWhenEachPairIsMatched()
        {
            var board = new MemoramaBoard(new[] { "rabbit", "dog", "pig", "rabbit", "dog", "pig" });

            board.Select(0);
            board.Select(3);
            board.Select(1);
            board.Select(4);
            board.Select(2);
            board.Select(5);

            Assert.AreEqual(6, board.CardCount);
            Assert.AreEqual(3, board.MatchedPairs);
            Assert.AreEqual(MemoramaPhase.Complete, board.Phase);
        }

        [Test]
        public void FourPairBoard_CompletesWhenEachPairIsMatched()
        {
            var board = new MemoramaBoard(new[] { "cat", "sheep", "rabbit", "dog", "cat", "sheep", "rabbit", "dog" });

            board.Select(0);
            board.Select(4);
            board.Select(1);
            board.Select(5);
            board.Select(2);
            board.Select(6);
            board.Select(3);
            board.Select(7);

            Assert.AreEqual(8, board.CardCount);
            Assert.AreEqual(4, board.MatchedPairs);
            Assert.AreEqual(MemoramaPhase.Complete, board.Phase);
        }

        [Test]
        public void SixPairBoard_CompletesWhenEachPairIsMatched()
        {
            var board = new MemoramaBoard(new[]
            {
                "cow", "rabbit", "dog", "sheep", "pig", "cat",
                "cow", "rabbit", "dog", "sheep", "pig", "cat"
            });

            for (int index = 0; index < 6; index++)
            {
                board.Select(index);
                board.Select(index + 6);
            }

            Assert.AreEqual(12, board.CardCount);
            Assert.AreEqual(6, board.MatchedPairs);
            Assert.AreEqual(MemoramaPhase.Complete, board.Phase);
        }

        private static MemoramaBoard CreateBoard() => new(new[] { "cow", "pig", "cow", "pig" });
    }
}
