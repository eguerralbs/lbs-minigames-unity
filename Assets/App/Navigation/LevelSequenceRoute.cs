namespace Lbs.MiniGames.Navigation
{
    public static class LevelSequenceRoute
    {
        public const string FunnyFaceDragGameId = "funnyface.drag";
        public const string ShapeAnalogyGameId = "shape.analogy";
        public const string WolfieFlasksGameId = "wolfie.flasks";
        public const string Thinking3DGameId = "thinking.3d";
        public const string AnimalDragGameId = "animal.drag";
        public const string KitchenMathLogicGameId = "kitchen.math.logic";
        public const string TrianglesShapeLogicGameId = "triangles.shape.logic";
        public const string CircleMathGameId = "circle.math";
        public const string CubePlatformGameId = "cube.platform";
        public const string ChemistrySelectionGameId = "chemistry.selection";
        public const string CandiesLogicGameId = "candies.logic";
        public const string ClothesSelectionGameId = "clothes.selection";
        public const string MakeAnEmojiDragGameId = "make.emoji.drag";
        public const string LadyBugPlaceGameId = "ladybug.place";
        public const string StickersPlacementGameId = "stickers.placement";
        public const string ObjectSelectionGameId = "object.selection";
        public const string BubbleMathGameId = "bubble.math";
        public const string TrianglesCountGameId = "triangles.count";
        public const string ThinkingFiguresGameId = "thinking.figures";
        public const string SquaresSuccessionGameId = "squares.succession";
        public const string FractionSuccessionGameId = "fraction.succession";
        public const string ShortestRouteGameId = "shortest.route";
        public const string AppleMathGameId = "apple.math";
        public const string ShapePutGameId = "shape.put";
        public const string AgeCompareGameId = "age.compare";

        public const string FunnyFaceDragSuccessTarget = ShapeAnalogyGameId;
        public const string ShapeAnalogySuccessTarget = WolfieFlasksGameId;
        public const string WolfieFlasksSuccessTarget = Thinking3DGameId;
        public const string Thinking3DSuccessTarget = AnimalDragGameId;
        public const string AnimalDragSuccessTarget = KitchenMathLogicGameId;
        public const string KitchenMathLogicSuccessTarget = TrianglesShapeLogicGameId;
        public const string TrianglesShapeLogicSuccessTarget = CircleMathGameId;
        public const string CircleMathSuccessTarget = CubePlatformGameId;
        public const string CubePlatformSuccessTarget = ChemistrySelectionGameId;
        public const string ChemistrySelectionSuccessTarget = CandiesLogicGameId;
        public const string CandiesLogicSuccessTarget = ClothesSelectionGameId;
        public const string ClothesSelectionSuccessTarget = MakeAnEmojiDragGameId;
        public const string MakeAnEmojiDragSuccessTarget = LadyBugPlaceGameId;
        public const string LadyBugPlaceSuccessTarget = StickersPlacementGameId;
        public const string StickersPlacementSuccessTarget = ObjectSelectionGameId;
        public const string ObjectSelectionSuccessTarget = BubbleMathGameId;
        public const string BubbleMathSuccessTarget = TrianglesCountGameId;
        public const string TrianglesCountSuccessTarget = ThinkingFiguresGameId;
        public const string ThinkingFiguresSuccessTarget = SquaresSuccessionGameId;
        public const string SquaresSuccessionSuccessTarget = FractionSuccessionGameId;
        public const string FractionSuccessionSuccessTarget = ShortestRouteGameId;
        public const string ShortestRouteSuccessTarget = AppleMathGameId;
        public const string AppleMathSuccessTarget = ShapePutGameId;
        public const string ShapePutSuccessTarget = AgeCompareGameId;

        /// <summary>
        /// Explicit membership boundary for games that share the logic-sequence BGM.
        /// Add future logic-sequence game IDs here.
        /// </summary>
        public static bool IsLogicSequenceGame(string gameId)
        {
            return gameId == FunnyFaceDragGameId
                || gameId == ShapeAnalogyGameId
                || gameId == WolfieFlasksGameId
                || gameId == Thinking3DGameId
                || gameId == AnimalDragGameId
                || gameId == KitchenMathLogicGameId
                || gameId == TrianglesShapeLogicGameId
                || gameId == CircleMathGameId
                || gameId == CubePlatformGameId
                || gameId == ChemistrySelectionGameId
                || gameId == CandiesLogicGameId
                || gameId == ClothesSelectionGameId
                || gameId == MakeAnEmojiDragGameId
                || gameId == LadyBugPlaceGameId
                || gameId == StickersPlacementGameId
                || gameId == ObjectSelectionGameId
                || gameId == BubbleMathGameId
                || gameId == TrianglesCountGameId
                || gameId == ThinkingFiguresGameId
                || gameId == SquaresSuccessionGameId
                || gameId == FractionSuccessionGameId
                || gameId == ShortestRouteGameId
                || gameId == AppleMathGameId
                || gameId == ShapePutGameId
                || gameId == AgeCompareGameId;
        }
    }
}
