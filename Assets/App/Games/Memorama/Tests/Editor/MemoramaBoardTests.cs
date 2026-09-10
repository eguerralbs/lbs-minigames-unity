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

        private static MemoramaBoard CreateBoard() => new(new[] { "cow", "pig", "cow", "pig" });
    }
}
