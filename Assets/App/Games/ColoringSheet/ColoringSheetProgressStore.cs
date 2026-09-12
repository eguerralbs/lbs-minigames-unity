using System;
using System.IO;
using UnityEngine;

namespace Lbs.MiniGames.Games.ColoringSheet
{
    /// <summary>Stores one validated local worksheet and its derived Hub preview.</summary>
    public sealed class ColoringSheetProgressStore
    {
        private const string Magic = "CSPS";
        private const int FormatVersion = 1;
        private const int MaximumPayloadBytes = 32 * 1024 * 1024;

        private readonly string gameId;
        private readonly string sourceId;
        private readonly int width;
        private readonly int height;
        private readonly string savePath;

        public ColoringSheetProgressStore(string gameId, string sourceId, int width, int height, string storageDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(gameId)) throw new ArgumentException("A game ID is required.", nameof(gameId));
            if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("A source ID is required.", nameof(sourceId));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            this.gameId = gameId;
            this.sourceId = sourceId;
            this.width = width;
            this.height = height;
            savePath = Path.Combine(storageDirectory ?? Application.persistentDataPath, gameId + ".coloring-progress");
        }

        public bool HasSavedProgress => File.Exists(savePath);
        public string SavePath => savePath;

        public bool Save(Texture2D paintTexture, Texture2D previewTexture)
        {
            if (paintTexture == null || previewTexture == null
                || paintTexture.width != width || paintTexture.height != height
                || previewTexture.width != width || previewTexture.height != height)
            {
                return false;
            }

            string temporaryPath = savePath + ".tmp";
            try
            {
                byte[] paintPng = paintTexture.EncodeToPNG();
                byte[] previewPng = previewTexture.EncodeToPNG();
                if (!IsValidPayloadLength(paintPng) || !IsValidPayloadLength(previewPng)) return false;

                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                using (FileStream stream = new(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (BinaryWriter writer = new(stream))
                {
                    writer.Write(Magic);
                    writer.Write(FormatVersion);
                    writer.Write(gameId);
                    writer.Write(sourceId);
                    writer.Write(width);
                    writer.Write(height);
                    writer.Write(paintPng.Length);
                    writer.Write(paintPng);
                    writer.Write(previewPng.Length);
                    writer.Write(previewPng);
                }

                File.Copy(temporaryPath, savePath, true);
                File.Delete(temporaryPath);
                return true;
            }
            catch (Exception)
            {
                TryDelete(temporaryPath);
                return false;
            }
        }

        public bool TryLoadPixels(out Color32[] pixels)
        {
            pixels = null;
            if (!TryReadPayload(out Payload payload)) return false;

            Texture2D texture = Decode(payload.PaintPng);
            if (texture == null) return RejectSavedProgress();
            try
            {
                if (texture.width != width || texture.height != height) return RejectSavedProgress();
                pixels = texture.GetPixels32();
                if (pixels.Length != width * height)
                {
                    pixels = null;
                    return RejectSavedProgress();
                }

                return true;
            }
            catch (Exception)
            {
                return RejectSavedProgress();
            }
            finally
            {
                UnityEngine.Object.Destroy(texture);
            }

        }

        public bool TryCreatePreviewSprite(out Sprite sprite)
        {
            sprite = null;
            if (!TryReadPayload(out Payload payload)) return false;

            Texture2D paintTexture = Decode(payload.PaintPng);
            Texture2D previewTexture = Decode(payload.PreviewPng);
            if (paintTexture == null || previewTexture == null
                || paintTexture.width != width || paintTexture.height != height
                || previewTexture.width != width || previewTexture.height != height)
            {
                if (paintTexture != null) UnityEngine.Object.Destroy(paintTexture);
                if (previewTexture != null) UnityEngine.Object.Destroy(previewTexture);
                return RejectSavedProgress();
            }

            UnityEngine.Object.Destroy(paintTexture);
            try
            {
                previewTexture.filterMode = FilterMode.Bilinear;
                previewTexture.wrapMode = TextureWrapMode.Clamp;
                sprite = Sprite.Create(previewTexture, new Rect(0f, 0f, previewTexture.width, previewTexture.height), new Vector2(0.5f, 0.5f));
                return true;
            }
            catch (Exception)
            {
                UnityEngine.Object.Destroy(previewTexture);
                return RejectSavedProgress();
            }
        }

        public void Delete()
        {
            TryDelete(savePath);
            TryDelete(savePath + ".tmp");
        }

        public static Texture2D CreatePreview(Texture2D paintTexture, Texture2D lineArtTexture, RectInt artworkRect)
        {
            if (paintTexture == null) throw new ArgumentNullException(nameof(paintTexture));
            if (lineArtTexture == null) throw new ArgumentNullException(nameof(lineArtTexture));
            if (artworkRect.width <= 0 || artworkRect.height <= 0) throw new ArgumentOutOfRangeException(nameof(artworkRect));

            int previewWidth = paintTexture.width;
            int previewHeight = paintTexture.height;
            Color32[] composed = new Color32[previewWidth * previewHeight];
            for (int index = 0; index < composed.Length; index++) composed[index] = Color.white;

            Color32[] paintPixels = paintTexture.GetPixels32();
            for (int index = 0; index < composed.Length; index++) composed[index] = BlendOver(composed[index], paintPixels[index]);

            Color32[] lineArtPixels = lineArtTexture.GetPixels32();
            for (int y = Mathf.Max(0, artworkRect.yMin); y < Mathf.Min(previewHeight, artworkRect.yMax); y++)
            {
                int sourceY = Mathf.Clamp((y - artworkRect.y) * lineArtTexture.height / artworkRect.height, 0, lineArtTexture.height - 1);
                for (int x = Mathf.Max(0, artworkRect.xMin); x < Mathf.Min(previewWidth, artworkRect.xMax); x++)
                {
                    int sourceX = Mathf.Clamp((x - artworkRect.x) * lineArtTexture.width / artworkRect.width, 0, lineArtTexture.width - 1);
                    int destinationIndex = y * previewWidth + x;
                    composed[destinationIndex] = BlendOver(composed[destinationIndex], lineArtPixels[sourceY * lineArtTexture.width + sourceX]);
                }
            }

            Texture2D preview = new(previewWidth, previewHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            preview.SetPixels32(composed);
            preview.Apply(false);
            return preview;
        }

        private bool TryReadPayload(out Payload payload)
        {
            payload = default;
            if (!File.Exists(savePath)) return false;
            try
            {
                using FileStream stream = new(savePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using BinaryReader reader = new(stream);
                if (reader.ReadString() != Magic || reader.ReadInt32() != FormatVersion || reader.ReadString() != gameId || reader.ReadString() != sourceId || reader.ReadInt32() != width || reader.ReadInt32() != height)
                {
                    return RejectSavedProgress();
                }

                byte[] paintPng = ReadPayload(reader);
                byte[] previewPng = ReadPayload(reader);
                if (paintPng == null || previewPng == null || stream.Position != stream.Length) return RejectSavedProgress();
                payload = new Payload(paintPng, previewPng);
                return true;
            }
            catch (Exception)
            {
                return RejectSavedProgress();
            }
        }

        private static Texture2D Decode(byte[] png)
        {
            if (!IsValidPayloadLength(png)) return null;
            Texture2D texture = null;
            try
            {
                texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (ImageConversion.LoadImage(texture, png, false)) return texture;
            }
            catch (Exception)
            {
                // Invalid image bytes are normal recovery input, not a game failure.
            }

            if (texture != null) UnityEngine.Object.Destroy(texture);
            return null;
        }

        private static byte[] ReadPayload(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            if (length <= 0 || length > MaximumPayloadBytes || length > reader.BaseStream.Length - reader.BaseStream.Position) return null;
            byte[] bytes = reader.ReadBytes(length);
            return bytes.Length == length ? bytes : null;
        }

        private bool RejectSavedProgress()
        {
            Delete();
            return false;
        }

        private static bool IsValidPayloadLength(byte[] payload) => payload != null && payload.Length > 0 && payload.Length <= MaximumPayloadBytes;
        private static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch (Exception) { } }

        private readonly struct Payload
        {
            public Payload(byte[] paintPng, byte[] previewPng) { PaintPng = paintPng; PreviewPng = previewPng; }
            public byte[] PaintPng { get; }
            public byte[] PreviewPng { get; }
        }

        private static Color32 BlendOver(Color32 destination, Color32 source)
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
    }
}
