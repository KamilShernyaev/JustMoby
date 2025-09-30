using UnityEngine;
using UnityEngine.EventSystems;
using Element;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Localization.Plugins.XLIFF.V20;
using Zones.DropZones.Tower;

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

            view.Initialize(this);
        }

        public void StartDrag(ElementModel elementModel, PointerEventData eventData)
        {
            model.ElementType = elementModel.ElementType;
            view.Show(model.ElementType.Sprite, eventData.position);
            eventData.pointerDrag = view.gameObject;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            view.SetPosition(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var dropped = dropZones.Any(zone =>
                zone.IsInsideZone(eventData.position) && zone.TryDropElement(model, view, eventData.position));

            if (!dropped)
            {
                view.FadeOutAndHide(0.3f);
                _ = notificationService.ShowNotification("MissCube");
            }
            else
            {
                view.Hide();
            }
        }
    }
}