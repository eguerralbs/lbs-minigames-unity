using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.ShapePut;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class ShapePutRuleTests
    {
        [Test]
        public void PurpleSquareIsTheThirdShape()
        {
            Assert.IsTrue(ShapePutRule.IsCorrect(ShapePutRule.Option2));
            Assert.IsFalse(ShapePutRule.IsCorrect(ShapePutRule.Option1));
            Assert.IsFalse(ShapePutRule.IsCorrect(ShapePutRule.Option3));
            Assert.IsFalse(ShapePutRule.IsCorrect(ShapePutRule.Option4));
        }

        [Test]
        public void WrongAnswerAppliesTheMistakeTier()
        {
            SelectionGameState state = new();

            Assert.IsFalse(state.Select(ShapePutRule.Option1, ShapePutRule.CorrectAnswer));
            Assert.IsTrue(state.HasMistake);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
        }
    }
}
