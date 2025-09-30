using Element;
using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;

namespace Services.DragService
{
    public class DraggingElementView : ElementView, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private DragController controller;

        private void Awake() => gameObject.SetActive(false);

        public void Initialize(DragController controller)
        {
            this.controller = controller;
        }

        public void Show(Sprite sprite, Vector3 position)
        {
            SetSprite(sprite);
            SetPosition(position);

            canvasGroup.alpha = 0.5f;
            gameObject.SetActive(true);
        }

        public void FadeOutAndHide(float duration)
        {
            if (canvasGroup == null) return;

            Tween.Alpha(canvasGroup, 0f, duration, Ease.InQuad)
                .OnComplete(Hide);
        }

        public void Hide() => gameObject.SetActive(false);
        public void SetPosition(Vector3 position) => transform.position = position;
        public void OnBeginDrag(PointerEventData eventData) => controller?.OnBeginDrag(eventData);
        public void OnDrag(PointerEventData eventData) => controller?.OnDrag(eventData);
        public void OnEndDrag(PointerEventData eventData) => controller?.OnEndDrag(eventData);
    }
}