namespace Lbs.MiniGames.Games.ShapePut
{
    public static class ShapePutRule
    {
        public const string Option1 = "option1";
        public const string Option2 = "option2";
        public const string Option3 = "option3";
        public const string Option4 = "option4";
        public const string CorrectAnswer = Option2;

        public static bool IsCorrect(string answerId) => answerId == CorrectAnswer;
    }
}
