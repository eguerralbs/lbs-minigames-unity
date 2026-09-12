using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lbs.MiniGames.Games.ColoringSheet
{
    /// <summary>Forwards uGUI pointer gestures from the worksheet background to the game adapter.</summary>
    public sealed class ColoringPaintSurface : RawImage, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private Action<PointerEventData> pointerDown;
        private Action<PointerEventData> pointerDrag;
        private Action<PointerEventData> pointerUp;

        public void Configure(Action<PointerEventData> onPointerDown, Action<PointerEventData> onPointerDrag, Action<PointerEventData> onPointerUp)
        {
            pointerDown = onPointerDown;
            pointerDrag = onPointerDrag;
            pointerUp = onPointerUp;
        }

        public void OnPointerDown(PointerEventData eventData) => pointerDown?.Invoke(eventData);
        public void OnDrag(PointerEventData eventData) => pointerDrag?.Invoke(eventData);
        public void OnPointerUp(PointerEventData eventData) => pointerUp?.Invoke(eventData);
    }
}
