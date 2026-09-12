using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.AgeCompare;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class AgeCompareRuleTests
    {
        [Test]
        public void CorrectAnswerIsOne()
        {
            Assert.IsTrue(AgeCompareRule.IsCorrect(AgeCompareRule.Option1));
            Assert.IsFalse(AgeCompareRule.IsCorrect(AgeCompareRule.Option7));
            Assert.IsFalse(AgeCompareRule.IsCorrect(AgeCompareRule.Option6));
            Assert.IsFalse(AgeCompareRule.IsCorrect(AgeCompareRule.Option5));
        }

        [Test]
        public void WrongAnswerAppliesTheMistakeTier()
        {
            SelectionGameState state = new();

            Assert.IsFalse(state.Select(AgeCompareRule.Option7, AgeCompareRule.CorrectAnswer));
            Assert.IsTrue(state.HasMistake);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
        }
    }
}
