namespace Lbs.MiniGames.Games.AgeCompare
{
    public static class AgeCompareRule
    {
        public const string Option7 = "7";
        public const string Option6 = "6";
        public const string Option5 = "5";
        public const string Option1 = "1";
        public const string CorrectAnswer = Option1;

        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}
