namespace Lbs.MiniGames.Games.ShortestRoute
{
    public static class ShortestRouteRule
    {
        public const string Character1 = "character1";
        public const string Character2 = "character2";
        public const string CorrectAnswer = Character1;

        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}
