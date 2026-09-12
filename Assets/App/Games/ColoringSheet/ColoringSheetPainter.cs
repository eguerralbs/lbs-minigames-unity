using System;
using UnityEngine;

namespace Lbs.MiniGames.Games.ColoringSheet
{
    /// <summary>Rasterizes bounded brush strokes while enforcing worksheet region ownership.</summary>
    public sealed class ColoringSheetPainter
    {
        private readonly Color32[] regionMap;
        private readonly Vector2 sheetSize;
        private readonly int brushRadiusPixels;
        private readonly Color32[] pixels;

        public ColoringSheetPainter(int width, int height, Vector2 sheetSize, Color32[] regionMap, int brushRadiusPixels = 7)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (sheetSize.x <= 0f || sheetSize.y <= 0f) throw new ArgumentOutOfRangeException(nameof(sheetSize));
            if (brushRadiusPixels <= 0) throw new ArgumentOutOfRangeException(nameof(brushRadiusPixels));
            Width = width;
            Height = height;
            this.sheetSize = sheetSize;
            this.brushRadiusPixels = brushRadiusPixels;
            if (regionMap == null) throw new ArgumentNullException(nameof(regionMap));
            if (regionMap.Length != width * height) throw new ArgumentException("The region map dimensions must match the paint buffer.", nameof(regionMap));
            this.regionMap = regionMap;
            MaxRegionId = GetMaxRegionId(regionMap);
            pixels = new Color32[width * height];
        }

        public int Width { get; }
        public int Height { get; }
        public int BackgroundRegionId => ColoringSheetRegionMapBuilder.BackgroundRegionId;
        public int MaxRegionId { get; }
        public Color32[] Pixels => pixels;

        public int GetRegionAt(Vector2 localPoint)
        {
            return TryLocalToPixel(localPoint, out int x, out int y) ? GetRegionAtPixel(x, y) : -1;
        }

        public int GetRegionAtPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return -1;
            Color32 region = regionMap[y * Width + x];
            return region.a == 0 || region.r == 0 ? -1 : region.r;
        }

        public bool PaintDab(Vector2 localPoint, int activeRegionIndex, Color32 color)
        {
            bool changed = false;
            Vector2 center = LocalToPixel(localPoint);
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - brushRadiusPixels));
            int maxX = Mathf.Min(Width - 1, Mathf.CeilToInt(center.x + brushRadiusPixels));
            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - brushRadiusPixels));
            int maxY = Mathf.Min(Height - 1, Mathf.CeilToInt(center.y + brushRadiusPixels));
            float radiusSquared = brushRadiusPixels * brushRadiusPixels;
            for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
            {
                float distanceSquared = (x + 0.5f - center.x) * (x + 0.5f - center.x) + (y + 0.5f - center.y) * (y + 0.5f - center.y);
                if (distanceSquared > radiusSquared || GetRegionAtPixel(x, y) != activeRegionIndex) continue;
                int pixelIndex = y * Width + x;
                pixels[pixelIndex] = Blend(pixels[pixelIndex], color);
                changed = true;
            }
            return changed;
        }

        public bool PaintStroke(Vector2 from, Vector2 to, int activeRegionIndex, Color32 color)
        {
            float pixelsPerUnit = Mathf.Min(Width / sheetSize.x, Height / sheetSize.y);
            float maxStep = brushRadiusPixels / pixelsPerUnit * 0.5f;
            int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(from, to) / maxStep));
            bool changed = false;
            for (int step = 0; step <= steps; step++) changed |= PaintDab(Vector2.Lerp(from, to, step / (float)steps), activeRegionIndex, color);
            return changed;
        }

        public Color32 GetPixel(int x, int y) => pixels[y * Width + x];
        public void Clear() => Array.Clear(pixels, 0, pixels.Length);

        private Vector2 LocalToPixel(Vector2 localPoint) => new((localPoint.x + sheetSize.x * 0.5f) * Width / sheetSize.x, (localPoint.y + sheetSize.y * 0.5f) * Height / sheetSize.y);

        private bool TryLocalToPixel(Vector2 localPoint, out int x, out int y)
        {
            float normalizedX = (localPoint.x + sheetSize.x * 0.5f) / sheetSize.x;
            float normalizedY = (localPoint.y + sheetSize.y * 0.5f) / sheetSize.y;
            x = Mathf.FloorToInt(normalizedX * Width);
            y = Mathf.FloorToInt(normalizedY * Height);
            return normalizedX >= 0f && normalizedX < 1f && normalizedY >= 0f && normalizedY < 1f;
        }

        private static Color32 Blend(Color32 destination, Color32 source)
        {
            float sourceAlpha = source.a / 255f;
            float destinationAlpha = destination.a / 255f;
            float outputAlpha = sourceAlpha + destinationAlpha * (1f - sourceAlpha);
            if (outputAlpha <= 0f) return default;
            return new Color32(
                (byte)Mathf.RoundToInt((source.r * sourceAlpha + destination.r * destinationAlpha * (1f - sourceAlpha)) / outputAlpha),
                (byte)Mathf.RoundToInt((source.g * sourceAlpha + destination.g * destinationAlpha * (1f - sourceAlpha)) / outputAlpha),
                (byte)Mathf.RoundToInt((source.b * sourceAlpha + destination.b * destinationAlpha * (1f - sourceAlpha)) / outputAlpha),
                (byte)Mathf.RoundToInt(outputAlpha * 255f));
        }

        private static int GetMaxRegionId(Color32[] map)
        {
            int maximum = 0;
            foreach (Color32 pixel in map) if (pixel.a != 0) maximum = Mathf.Max(maximum, pixel.r);
            return maximum;
        }
    }
}
