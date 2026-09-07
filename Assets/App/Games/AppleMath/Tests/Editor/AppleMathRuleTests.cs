using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.AppleMath;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class AppleMathRuleTests
    {
        [Test]
        public void EachAppleIsWorthTwo()
        {
            Assert.AreEqual(2, AppleMathRule.AppleValue);
            Assert.IsTrue(AppleMathRule.IsCorrect(AppleMathRule.Option2));
            Assert.IsFalse(AppleMathRule.IsCorrect(AppleMathRule.Option3));
            Assert.IsFalse(AppleMathRule.IsCorrect(AppleMathRule.Option1));
        }

        [Test]
        public void WrongAnswerAppliesTheMistakeTier()
        {
            SelectionGameState state = new();

            Assert.IsFalse(state.Select(AppleMathRule.Option3, AppleMathRule.CorrectAnswer));
            Assert.IsTrue(state.HasMistake);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
        }
    }
}
