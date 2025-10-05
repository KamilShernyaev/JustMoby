using UnityEngine.EventSystems;
using Element;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using Services.AnimationService;
using Services.NotificationService;

namespace Services.DragService
{
    public class DragController : Controller<DraggingElementModel, DraggingElementView>
    {
        private readonly IReadOnlyList<IDropZone> dropZones;
        private readonly INotificationService notificationService;
        private readonly IAnimationService animationService;
        private readonly Canvas canvas;

        public DragController(DraggingElementModel model, DraggingElementView view, IReadOnlyList<IDropZone> dropZones,
            INotificationService notificationService, IAnimationService animationService, Canvas canvas)
            : base(model, view)
        {
            this.dropZones = dropZones;
            this.notificationService = notificationService;
            this.animationService = animationService;
            this.canvas = canvas;

            view.OnDragEvent += eventData => OnDrag(view, eventData);
            view.OnEndDragEvent += eventData => OnEndDrag(model, view, eventData);
            view.OnRemoveRequested += ViewOnOnRemoveRequested;
        }

        private void ViewOnOnRemoveRequested(ElementModel obj)
        {
            Model.OriginalView.OnRemove(Model.OriginalModel);
        }

        public void StartDrag(ElementModel elementModel, ElementView elementView, PointerEventData eventData)
        {
            Model.ElementType = elementModel.ElementType;
            Model.OriginalModel = elementModel;
            Model.OriginalView = elementView;
            View.Show(Model.ElementType.Sprite, elementView.transform.position);
            eventData.pointerDrag = View.gameObject;
        }

        private void OnDrag(DraggingElementView view, PointerEventData eventData)
        {
            view.SetPosition(eventData.position);
        }

        private void OnEndDrag(DraggingElementModel model, DraggingElementView view, PointerEventData eventData)
        {
            var targetZone = dropZones.FirstOrDefault(zone => zone.IsInsideZone(eventData.position));
            if (targetZone == null)
            {
                animationService.PlayFade(view.transform, false, 0.3f, view.Hide);
                _ = notificationService.ShowNotification("MissCube");
                return;
            }

            var startLocalPos = ConvertWorldToLocalCanvas(model.OriginalView.transform.position);
            var targetLocalPos = ConvertScreenToLocalCanvas(eventData.position);

            var isTowerDropFromTower = false;
            if (targetZone is Zones.DropZones.Hole.HoleController hole)
            {
                var holeWorldPos = hole.View.HoleImage.rectTransform.position;
                targetLocalPos = ConvertWorldToLocalCanvas(holeWorldPos);
                _ = notificationService.ShowNotification("DropHole");
            }
            else if (targetZone is Zones.DropZones.Tower.TowerContainerController tower)
            {
                var towerModel = tower.Model;
                if (towerModel.ElementCount > 0)
                {
                    targetLocalPos += new Vector3(UnityEngine.Random.Range(-50f, 50f), 50f, 0);
                }

                _ = notificationService.ShowNotification("PlaceCube");
                isTowerDropFromTower = model.OriginalModel is Zones.DropZones.Tower.TowerElement.TowerElementModel;
            }

            var dropped = targetZone.TryDropElement(model.OriginalModel, model.OriginalView, eventData.position);
            if (!dropped)
            {
                var currentDropLocalPos = ConvertScreenToLocalCanvas(eventData.position);
                animationService.PlayFade(view.transform, false, 0.3f, () =>
                {
                    animationService.PlayJump(currentDropLocalPos, startLocalPos, model.ElementType.Sprite, 0.3f,
                        view.Hide, arc: false);
                });
                return;
            }

            view.Hide();
            animationService.PlayJump(startLocalPos, targetLocalPos, model.ElementType.Sprite, 0.5f,
                () =>
                {
                    if (!isTowerDropFromTower)
                        model.OriginalView.OnRemove(model.OriginalModel);
                });
        }

        private Vector3 ConvertWorldToLocalCanvas(Vector3 worldPos)
        {
            if (canvas == null) return worldPos;
            var canvasRt = canvas.GetComponent<RectTransform>();
            if (canvasRt == null) return worldPos;

            var localPos = canvasRt.InverseTransformPoint(worldPos);
            return new Vector3(localPos.x, localPos.y, 0);
        }

        private Vector3 ConvertScreenToLocalCanvas(Vector2 screenPos)
        {
            if (canvas == null) return new Vector3(screenPos.x, screenPos.y, 0);

            var canvasRt = canvas.GetComponent<RectTransform>();
            if (canvasRt == null) return new Vector3(screenPos.x, screenPos.y, 0);

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPos, null, out var localPos)
                ? new Vector3(localPos.x, localPos.y, 0)
                : new Vector3(screenPos.x, screenPos.y, 0);
        }
    }
}