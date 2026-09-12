using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lbs.MiniGames.Games.ColoringSheet
{
    /// <summary>Builds a board-sized region map with centered, aspect-preserving artwork.</summary>
    public static class ColoringSheetRegionMapBuilder
    {
        public const byte BackgroundRegionId = 1;

        public static Color32[] BuildPadded(
            Color32[] sourceMap,
            int sourceWidth,
            int sourceHeight,
            int boardWidth,
            int boardHeight,
            RectInt artworkRect)
        {
            if (sourceMap == null) throw new ArgumentNullException(nameof(sourceMap));
            if (sourceWidth <= 0) throw new ArgumentOutOfRangeException(nameof(sourceWidth));
            if (sourceHeight <= 0) throw new ArgumentOutOfRangeException(nameof(sourceHeight));
            if (boardWidth <= 0) throw new ArgumentOutOfRangeException(nameof(boardWidth));
            if (boardHeight <= 0) throw new ArgumentOutOfRangeException(nameof(boardHeight));
            if (sourceMap.Length != sourceWidth * sourceHeight) throw new ArgumentException("The source map dimensions must match its pixel buffer.", nameof(sourceMap));
            if (artworkRect.width <= 0 || artworkRect.height <= 0 || artworkRect.xMin < 0 || artworkRect.yMin < 0 || artworkRect.xMax > boardWidth || artworkRect.yMax > boardHeight)
                throw new ArgumentOutOfRangeException(nameof(artworkRect));

            Color32[] paddedMap = new Color32[boardWidth * boardHeight];
            Color32 background = new(BackgroundRegionId, 0, 0, byte.MaxValue);
            for (int index = 0; index < paddedMap.Length; index++) paddedMap[index] = background;

            for (int y = 0; y < artworkRect.height; y++)
            {
                int sourceY = y * sourceHeight / artworkRect.height;
                for (int x = 0; x < artworkRect.width; x++)
                {
                    int sourceX = x * sourceWidth / artworkRect.width;
                    paddedMap[(artworkRect.y + y) * boardWidth + artworkRect.x + x] = sourceMap[sourceY * sourceWidth + sourceX];
                }
            }

            return PromoteInnerEdgePixels(paddedMap, boardWidth, boardHeight);
        }

        private static Color32[] PromoteInnerEdgePixels(Color32[] map, int width, int height)
        {
            int[] backgroundDistances = FindBackgroundDistances(map, width, height);
            int[] regionDistances = new int[map.Length];
            int[] nearestRegions = new int[map.Length];
            bool[] ambiguousRegions = new bool[map.Length];
            Array.Fill(regionDistances, -1);
            Queue<int> pending = new();

            for (int index = 0; index < map.Length; index++)
            {
                if (!IsPaintableRegion(map[index]) || map[index].r == BackgroundRegionId) continue;
                regionDistances[index] = 0;
                nearestRegions[index] = map[index].r;
                pending.Enqueue(index);
            }

            while (pending.Count > 0)
            {
                int index = pending.Dequeue();
                VisitNeighbors(index, width, height, neighbor =>
                {
                    int distance = regionDistances[index] + 1;
                    if (regionDistances[neighbor] >= 0 && regionDistances[neighbor] <= distance)
                    {
                        if (regionDistances[neighbor] == distance)
                            ambiguousRegions[neighbor] |= ambiguousRegions[index] || nearestRegions[neighbor] != nearestRegions[index];
                        return;
                    }

                    regionDistances[neighbor] = distance;
                    nearestRegions[neighbor] = nearestRegions[index];
                    ambiguousRegions[neighbor] = ambiguousRegions[index];
                    pending.Enqueue(neighbor);
                });
            }

            for (int index = 0; index < map.Length; index++)
            {
                if (IsPaintableRegion(map[index]) || ambiguousRegions[index] || regionDistances[index] < 0) continue;
                if (backgroundDistances[index] >= 0 && regionDistances[index] >= backgroundDistances[index]) continue;
                map[index] = new Color32((byte)nearestRegions[index], 0, 0, byte.MaxValue);
            }

            return map;
        }

        private static int[] FindBackgroundDistances(Color32[] map, int width, int height)
        {
            int[] distances = new int[map.Length];
            Array.Fill(distances, -1);
            Queue<int> pending = new();
            for (int index = 0; index < map.Length; index++)
            {
                if (!IsPaintableRegion(map[index]) || map[index].r != BackgroundRegionId) continue;
                distances[index] = 0;
                pending.Enqueue(index);
            }

            while (pending.Count > 0)
            {
                int index = pending.Dequeue();
                VisitNeighbors(index, width, height, neighbor =>
                {
                    if (distances[neighbor] >= 0) return;
                    distances[neighbor] = distances[index] + 1;
                    pending.Enqueue(neighbor);
                });
            }

            return distances;
        }

        private static bool IsPaintableRegion(Color32 pixel) => pixel.a != 0 && pixel.r != 0;

        private static void VisitNeighbors(int index, int width, int height, Action<int> visitor)
        {
            int x = index % width;
            int y = index / width;
            if (x > 0) visitor(index - 1);
            if (x < width - 1) visitor(index + 1);
            if (y > 0) visitor(index - width);
            if (y < height - 1) visitor(index + width);
        }
    }
}
