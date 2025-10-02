using UnityEngine;
using UnityEngine.EventSystems;
using System;
using Core;
using Core.MVC;
using UnityEngine.UI;

namespace Element
{
    public class ElementView : View, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] protected Image image;
        [SerializeField] protected CanvasGroup canvasGroup;

        public event Action<PointerEventData> OnBeginDragEvent;
        public event Action<PointerEventData> OnDragEvent;
        public event Action<PointerEventData> OnEndDragEvent;
        public event Action<ElementModel> OnRemoveRequested;

        public void SetSprite(Sprite sprite)
        {
            if (image != null) image.sprite = sprite;
        }

        public void SetAlpha(float value)
        {
            if (canvasGroup != null) canvasGroup.alpha = value;
        }

        public virtual void OnBeginDrag(PointerEventData eventData) => OnBeginDragEvent?.Invoke(eventData);
        public virtual void OnDrag(PointerEventData eventData) => OnDragEvent?.Invoke(eventData);
        public virtual void OnEndDrag(PointerEventData eventData) => OnEndDragEvent?.Invoke(eventData);
        public virtual void OnRemove(ElementModel elementModel) => OnRemoveRequested?.Invoke(elementModel);
    }
}