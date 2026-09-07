using Lbs.MiniGames.GameKits.Selection;
using Lbs.MiniGames.Games.ShortestRoute;
using NUnit.Framework;

namespace Lbs.MiniGames.Tests
{
    public sealed class ShortestRouteRuleTests
    {
        [Test]
        public void Character1_IsTheShortestBlueRoute()
        {
            Assert.IsTrue(ShortestRouteRule.IsCorrect(ShortestRouteRule.Character1));
            Assert.IsFalse(ShortestRouteRule.IsCorrect(ShortestRouteRule.Character2));
        }

        [Test]
        public void WrongCharacter_AppliesTheMistakeTier()
        {
            SelectionGameState state = new();

            Assert.IsFalse(state.Select(ShortestRouteRule.Character2, ShortestRouteRule.CorrectAnswer));
            Assert.IsTrue(state.HasMistake);
            Assert.AreEqual(4, state.Score);
            Assert.AreEqual(1, state.StarCount);
        }
    }
}
