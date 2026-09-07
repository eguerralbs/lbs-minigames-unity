using System;
using Lbs.MiniGames.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Lbs.MiniGames.Shared.UI
{
    /// <summary>
    /// Factory for semantic reusable Exit + Hong composition. Compatible with current procedural uGUI.
    /// Centralizes approved coordinates and touch sizes; no duplicate literals in callers.
    /// </summary>
    public static class LevelChromeFactory
    {
        public static LevelChrome Build(
            RectTransform parent,
            Font font,
            Sprite exitSprite,
            Sprite hongSprite,
            Action onExit,
            Action onHong)
        {
            return Build(parent, font, exitSprite, hongSprite, onExit, onHong, null);
        }

        public static LevelChrome Build(
            RectTransform parent,
            Font font,
            Sprite exitSprite,
            Sprite hongSprite,
            Action onExit,
            Action onHong,
            LevelChromeConfig config)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));

            Vector2 exitCenter = config != null ? config.ExitCenter : LevelChromeLayout.ExitCenter;
            Vector2 exitSize = config != null ? config.ExitSize : LevelChromeLayout.ExitSize;
            Vector2 hongCenter = config != null ? config.HongCenter : LevelChromeLayout.HongCenter;
            Vector2 hongSize = config != null ? config.HongSize : LevelChromeLayout.HongSize;
            float hongRadius = config != null ? config.HongCornerRadius : 28f;

            // Exit button (clear background, artwork stretched, shadow)
            Button exit = UiFactory.CreateButton(parent, "Exit", font, string.Empty, Color.clear);
            RectTransform exitRect = exit.GetComponent<RectTransform>();
            PixelRect(exitRect, exitCenter, exitSize);
            // Hide label created by CreateButton
            Text exitLabel = exit.GetComponentInChildren<Text>();
            if (exitLabel != null) exitLabel.gameObject.SetActive(false);
            Image exitImage = UiFactory.CreateImage(exitRect, "ExitArtwork", Color.white);
            exitImage.sprite = exitSprite;
            exitImage.preserveAspect = true;
            AddArtworkShadow(exitImage, LevelChromeLayout.ArtworkShadowOffset, LevelChromeLayout.ArtworkShadowAlpha);
            UiFactory.Stretch(exitImage.rectTransform, 0f);
            // Tint the artwork itself so the press is visible; the clear
            // background graphic would swallow the default color transition.
            exit.targetGraphic = exitImage;
            exit.transition = Selectable.Transition.ColorTint;
            ColorBlock exitColors = exit.colors;
            exitColors.normalColor = Color.white;
            exitColors.highlightedColor = Color.white;
            exitColors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
            exitColors.selectedColor = Color.white;
            exitColors.disabledColor = Color.white;
            exitColors.fadeDuration = 0.1f;
            exit.colors = exitColors;

            // Hong / Speaker
            RoundedSurface hongSurface = UiFactory.CreateRoundedSurface(parent, "Hong", Color.clear, hongRadius);
            PixelRect(hongSurface.rectTransform, hongCenter, hongSize);
            Image hongArtwork = UiFactory.CreateImage(hongSurface.rectTransform, "HongArtwork", Color.white);
            hongArtwork.sprite = hongSprite;
            hongArtwork.preserveAspect = true;
            AddArtworkShadow(hongArtwork, LevelChromeLayout.ArtworkShadowOffset, LevelChromeLayout.ArtworkShadowAlpha);
            UiFactory.Stretch(hongArtwork.rectTransform, 0f);
            Button hong = hongSurface.gameObject.AddComponent<Button>();
            hong.targetGraphic = hongSurface;

            GameObject chromeObject = new("LevelChrome", typeof(LevelChrome));
            chromeObject.transform.SetParent(parent, false);
            // Keep chrome controller not interfering with layout; it just owns references.
            // Move chrome object to be non-visual anchor.
            LevelChrome controller = chromeObject.GetComponent<LevelChrome>();
            controller.Configure(exit, hong, hongArtwork, onExit, onHong);

            return controller;
        }

        private static void PixelRect(RectTransform rect, Vector2 topOriginCenter, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = LevelChromeLayout.ToAnchoredPosition(topOriginCenter);
            rect.sizeDelta = size;
        }

        private static void AddArtworkShadow(Image image, float offset, float alpha)
        {
            if (image == null) return;
            Shadow shadow = image.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, alpha);
            shadow.effectDistance = new Vector2(offset, -offset);
            shadow.useGraphicAlpha = true;
        }
    }
}
