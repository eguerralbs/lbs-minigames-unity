using System;

namespace Lbs.MiniGames.Games.ColoringSheet
{
    /// <summary>Owns the transient palette and region assignments for one coloring session.</summary>
    public sealed class ColoringSheetState
    {
        private readonly int[] regionPaletteIndices;

        public ColoringSheetState(int regionCount, int paletteCount, int defaultPaletteIndex = 0)
        {
            if (regionCount <= 0) throw new ArgumentOutOfRangeException(nameof(regionCount));
            if (paletteCount <= 0) throw new ArgumentOutOfRangeException(nameof(paletteCount));
            if (defaultPaletteIndex < 0 || defaultPaletteIndex >= paletteCount) throw new ArgumentOutOfRangeException(nameof(defaultPaletteIndex));

            PaletteCount = paletteCount;
            SelectedPaletteIndex = defaultPaletteIndex;
            regionPaletteIndices = new int[regionCount];
            Reset();
        }

        public int PaletteCount { get; }
        public int RegionCount => regionPaletteIndices.Length;
        public int SelectedPaletteIndex { get; private set; }

        public int GetRegionPaletteIndex(int regionIndex) => IsValidRegion(regionIndex) ? regionPaletteIndices[regionIndex] : -1;

        public void SelectPalette(int paletteIndex)
        {
            if (paletteIndex < 0 || paletteIndex >= PaletteCount) throw new ArgumentOutOfRangeException(nameof(paletteIndex));
            SelectedPaletteIndex = paletteIndex;
        }

        public bool TryPaintRegion(int regionIndex)
        {
            if (!IsValidRegion(regionIndex)) return false;
            regionPaletteIndices[regionIndex] = SelectedPaletteIndex;
            return true;
        }

        public void Reset()
        {
            for (int index = 0; index < regionPaletteIndices.Length; index++) regionPaletteIndices[index] = 0;
            SelectedPaletteIndex = 0;
        }

        private bool IsValidRegion(int regionIndex) => regionIndex >= 0 && regionIndex < regionPaletteIndices.Length;
    }
}
