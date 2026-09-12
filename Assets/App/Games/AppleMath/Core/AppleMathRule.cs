namespace Lbs.MiniGames.Games.AppleMath
{
    public static class AppleMathRule
    {
        public const int AppleValue = 2;
        public const string Option3 = "3";
        public const string Option2 = "2";
        public const string Option1 = "1";
        public const string CorrectAnswer = Option2;

        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}
