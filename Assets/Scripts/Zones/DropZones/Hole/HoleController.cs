using Core;
using Element;
using Services.DragService;
using Services.NotificationService;
using UnityEngine;

namespace Zones.DropZones.Hole
{
    public class HoleController : Controller<HoleModel, HoleView>, IDropZone
    {
        private readonly NotificationService notificationService;

        public HoleController(HoleModel model, HoleView view, NotificationService notificationService)
            : base(model, view)
        {
            this.notificationService = notificationService;
        }

        public bool IsInsideZone(Vector3 screenPosition)
        {
            if (View == null) return false;
            var rectTransform = View.GetComponent<RectTransform>();
            return rectTransform != null &&
                   RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition);
        }

        public bool TryDropElement(ElementModel elementModel, ElementView elementView, Vector3 worlPosition)
        {
            if (elementView == null) return false;

            elementView.OnRemove(elementModel);
            _ = notificationService.ShowNotification("DropHole");
            return true;
        }
    }
}