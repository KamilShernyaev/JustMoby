using Core;
using Element;
using Services.ConfigProvider;
using Services.DragService;
using Services.NotificationService;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Zones.DropZones.Hole
{
    public class HoleController : Controller<HoleModel, HoleView>, IDropZone
    {
        private readonly INotificationService notificationService;

        public HoleController(HoleModel model, HoleView view, INotificationService notificationService,
            IConfigProvider configProvider) : base(model, view)
        {
            this.notificationService = notificationService;

            if (View != null && configProvider != null)
            {
                View.SetBackground(configProvider.GetBackgroundSprite(BackgroundZoneType.Hole));
            }

            InitializeHoleBounds();
        }

        public bool IsInsideZone(Vector3 screenPosition)
        {
            if (View == null || Model == null || View.HoleImage == null) return false;

            var rectTransform = View.HoleImage.rectTransform;
            if (rectTransform == null || Model.EllipseSize == Vector2.zero) return false;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, screenPosition, null, out var localPoint))
            {
                return false;
            }

            var hit = Model.IsPointInsideEllipse(localPoint);

            return hit;
        }

        private void InitializeHoleBounds()
        {
            if (View == null || Model == null || View.HoleImage == null) return;

            var rectTransform = View.HoleImage.rectTransform;
            if (rectTransform == null)
            {
                Debug.LogError("[HoleController InitBounds] RectTransform missing on HoleImage!");
                return;
            }

            var sizeDelta = rectTransform.sizeDelta;

            Model.EllipseSize = sizeDelta;

            var scale = rectTransform.localScale;
            Model.EllipseSize.x *= Mathf.Abs(scale.x);
            Model.EllipseSize.y *= Mathf.Abs(scale.y);
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