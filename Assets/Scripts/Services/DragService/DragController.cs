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

        public DragController(DraggingElementModel model, DraggingElementView view, IReadOnlyList<IDropZone> dropZones,
            INotificationService notificationService, IAnimationService animationService) : base(model, view)

        {
            this.dropZones = dropZones;
            this.notificationService = notificationService;
            this.animationService = animationService;

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
                // Miss no zone: Fade out dragging + hide (no jump back)
                animationService.PlayFade(view.transform, false, 0.3f, view.Hide);
                _ = notificationService.ShowNotification("MissCube");
                return;
            }

            Vector3 targetPosition = eventData.position;
            var startPosition = model.OriginalView.transform.position;
            if (targetZone is Zones.DropZones.Hole.HoleController hole)
            {
                targetPosition = hole.View.HoleImage.rectTransform.position;
                _ = notificationService.ShowNotification("DropHole");
            }
            else if (targetZone is Zones.DropZones.Tower.TowerContainerController tower)
            {
                if (tower.Model.ElementCount == 0)
                {
                    var rect = tower.View.GetComponent<RectTransform>().rect;
                    targetPosition = new Vector3(tower.Model.BasePosition.x, rect.yMin, 0);
                }
                else
                {
                    var topIndex = tower.Model.ElementCount - 1;
                    var topPos = tower.Model.GetElementPosition(topIndex, 0.5f);
                    targetPosition = new Vector3(topPos.x + UnityEngine.Random.Range(-50f, 50f), topPos.y + 50f, 0);
                }

                _ = notificationService.ShowNotification("PlaceCube");
            }

            var dropped = targetZone.TryDropElement(model.OriginalModel, model.OriginalView, eventData.position);
            if (!dropped)
            {
                // Miss after zone: Fade out dragging + linear jump back (pool view) + hide
                animationService.PlayFade(view.transform, false, 0.3f, () => // Fade dragging first
                {
                    // Linear jump back (visual copy from current to start)
                    animationService.PlayJump(eventData.position, startPosition, model.ElementType.Sprite, 0.3f,
                        view.Hide, arc: false); // Callback: Hide dragging (no extra fade)
                });
                _ = notificationService.ShowNotification("MissCube");
                return;
            }

            // Success: Unchanged
            view.Hide();
            animationService.PlayJump(startPosition, targetPosition, model.ElementType.Sprite, 0.5f,
                () => model.OriginalView.OnRemove(model.OriginalModel));
        }
    }
}