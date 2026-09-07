namespace Lbs.MiniGames.Navigation
{
    public static class LevelSequenceRoute
    {
        public const string ShapeAnalogyGameId = "shape.analogy";
        public const string ClothesSelectionGameId = "clothes.selection";
        public const string ObjectSelectionGameId = "object.selection";
        public const string MakeAnEmojiDragGameId = "make.emoji.drag";
        public const string AnimalDragGameId = "animal.drag";
        public const string TrianglesCountGameId = "triangles.count";
        public const string CubePlatformGameId = "cube.platform";
        public const string CandiesLogicGameId = "candies.logic";
        public const string SquaresSuccessionGameId = "squares.succession";
        public const string KitchenMathLogicGameId = "kitchen.math.logic";
        public const string FunnyFaceDragGameId = "funnyface.drag";
        public const string ChemistrySelectionGameId = "chemistry.selection";
        public const string TrianglesShapeLogicGameId = "triangles.shape.logic";
        public const string Thinking3DGameId = "thinking.3d";
        public const string CircleMathGameId = "circle.math";
        public const string BubbleMathGameId = "bubble.math";
        public const string LadyBugPlaceGameId = "ladybug.place";
        public const string FractionSuccessionGameId = "fraction.succession";

        public const string ShapeAnalogySuccessTarget = "clothes.selection";
        public const string ClothesSelectionSuccessTarget = "object.selection";
        public const string ObjectSelectionSuccessTarget = "make.emoji.drag";
        public const string MakeAnEmojiDragSuccessTarget = "animal.drag";
        public const string AnimalDragSuccessTarget = "triangles.count";
        public const string TrianglesCountSuccessTarget = "cube.platform";
        public const string CubePlatformSuccessTarget = "candies.logic";
        public const string CandiesLogicSuccessTarget = "squares.succession";
        public const string SquaresSuccessionSuccessTarget = "kitchen.math.logic";
        public const string KitchenMathLogicSuccessTarget = "funnyface.drag";
        public const string FunnyFaceDragSuccessTarget = "chemistry.selection";
        public const string ChemistrySelectionSuccessTarget = "triangles.shape.logic";
        public const string TrianglesShapeLogicSuccessTarget = "thinking.3d";
        public const string Thinking3DSuccessTarget = "circle.math";
        public const string CircleMathSuccessTarget = "bubble.math";
        public const string BubbleMathSuccessTarget = "ladybug.place";
        public const string LadyBugPlaceSuccessTarget = "fraction.succession";
        public const string FractionSuccessionSuccessTarget = "thinking.figures";
        public const string ThinkingFiguresGameId = "thinking.figures";
        public const string ThinkingFiguresSuccessTarget = "stickers.placement";
        public const string StickersPlacementGameId = "stickers.placement";
        public const string StickersPlacementSuccessTarget = "wolfie.flasks";
        public const string WolfieFlasksGameId = "wolfie.flasks";
        public const string ShortestRouteGameId = "shortest.route";
        public const string AppleMathGameId = "apple.math";
        public const string ShapePutGameId = "shape.put";
        public const string AgeCompareGameId = "age.compare";
        public const string WolfieFlasksSuccessTarget = "shortest.route";
        public const string ShortestRouteSuccessTarget = "apple.math";
        public const string AppleMathSuccessTarget = "shape.put";
        public const string ShapePutSuccessTarget = "age.compare";

        /// <summary>
        /// Explicit membership boundary for games that share the logic-sequence BGM.
        /// Add future logic-sequence game IDs here.
        /// </summary>
        public static bool IsLogicSequenceGame(string gameId)
        {
            return gameId == ShapeAnalogyGameId
                || gameId == ClothesSelectionGameId
                || gameId == ObjectSelectionGameId
                || gameId == MakeAnEmojiDragGameId
                || gameId == AnimalDragGameId
                || gameId == TrianglesCountGameId
                || gameId == CubePlatformGameId
                || gameId == CandiesLogicGameId
                || gameId == SquaresSuccessionGameId
                || gameId == KitchenMathLogicGameId
                || gameId == FunnyFaceDragGameId
                || gameId == ChemistrySelectionGameId
                || gameId == TrianglesShapeLogicGameId
                || gameId == Thinking3DGameId
                || gameId == CircleMathGameId
                || gameId == BubbleMathGameId
                || gameId == LadyBugPlaceGameId
                || gameId == FractionSuccessionGameId
                || gameId == ThinkingFiguresGameId
                || gameId == StickersPlacementGameId
                || gameId == WolfieFlasksGameId
                || gameId == ShortestRouteGameId
                || gameId == AppleMathGameId
                || gameId == ShapePutGameId
                || gameId == AgeCompareGameId;
        }
    }
}
