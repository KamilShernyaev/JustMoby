using UnityEngine.EventSystems;
using Element;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PrimeTween;

namespace Services.DragService
{
    public class DragController
    {
        private readonly DraggingElementModel model;
        private readonly DraggingElementView view;
        private readonly IReadOnlyList<IDropZone> dropZones;
        private readonly NotificationService.NotificationService notificationService;

        public DragController(DraggingElementModel model, DraggingElementView view, IReadOnlyList<IDropZone> dropZones,
            NotificationService.NotificationService notificationService)
        {
            this.model = model;
            this.view = view;
            this.dropZones = dropZones;
            this.notificationService = notificationService;

            view.OnDragEvent += eventData => OnDrag(model, view, eventData);
            view.OnEndDragEvent += eventData => OnEndDrag(model, view, eventData);
            view.OnRemoveRequested += ViewOnOnRemoveRequested;
        }

        private void ViewOnOnRemoveRequested(ElementModel obj)
        {
            model.OriginalView.OnRemove(model.OriginalModel);
        }

        public void StartDrag(ElementModel elementModel, ElementView elementView, PointerEventData eventData)
        {
            model.ElementType = elementModel.ElementType;
            model.OriginalModel = elementModel;
            model.OriginalView = elementView;
            view.Show(model.ElementType.Sprite, elementView.transform.position);
            eventData.pointerDrag = view.gameObject;
        }

        private void OnDrag(DraggingElementModel model, DraggingElementView view, PointerEventData eventData)
        {
            view.SetPosition(eventData.position);
        }

        private void OnEndDrag(DraggingElementModel model, DraggingElementView view, PointerEventData eventData)
        {
            var targetZone = dropZones.FirstOrDefault(zone => zone.IsInsideZone(eventData.position));
            if (targetZone == null)
            {
                view.FadeOutAndHide(0.3f);
                _ = notificationService.ShowNotification("MissCube");
                return;
            }


            Vector3 targetPosition = eventData.position;
            if (targetZone is Zones.DropZones.Tower.TowerContainerController tower)
            {
                var rectTransform = tower.View?.GetComponent<RectTransform>();
                if (rectTransform != null && tower.Model.ElementCount > 0)
                {
                    var topElement = tower.Model.GetElementAt(tower.Model.ElementCount - 1);
                    var topPos = tower.Model.GetElementPosition(tower.Model.ElementCount - 1, 0.5f);
                    RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, null,
                        out var worldPos);
                    targetPosition = new Vector3(worldPos.x, topPos.y + topElement.ElementHeight, worldPos.z);
                }
            }

            var startPosition = model.OriginalView != null
                ? model.OriginalView.transform.position
                : view.transform.position;


            var dropped = targetZone.TryDropElement(model, view, eventData.position);
            if (!dropped)
            {
                AnimateJumpToPosition(view, startPosition, targetPosition, () => { view.FadeOutAndHide(0.1f); });
                _ = notificationService.ShowNotification("MissCube");
            }
            else
            {
                view.Hide();
            }
        }

        private void AnimateJumpToPosition(DraggingElementView view, Vector3 startPosition, Vector3 targetPosition,
            System.Action onComplete)
        {
            if (view == null)
            {
                Debug.LogWarning("DraggingElementView is null");
                return;
            }

            var rectTransform = view.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogWarning("RectTransform is null on DraggingElementView");
                return;
            }

            view.SetAlpha(1);
            var startPos2D = new Vector2(startPosition.x, startPosition.y);
            var targetPos2D = new Vector2(targetPosition.x, targetPosition.y);
            const float duration = 0.3f;

            rectTransform.position = startPosition;

            Tween.Custom(0f, 1f, duration, t =>
                {
                    var x = Mathf.Lerp(startPos2D.x, targetPos2D.x, t);
                    var y = Mathf.Lerp(startPos2D.y, targetPos2D.y, t);

                    rectTransform.position = new Vector3(x, y, startPosition.z);
                }, Ease.Linear)
                .OnComplete(onComplete);
        }
    }
}