using Core;
using Element;
using Services.DragService;
using Services.NotificationService;
using Services.PoolService;
using UnityEngine;

namespace Zones.DropZones.Hole
{
    public class HoleController : Controller<HoleModel, HoleView>, IDropZone
    {
        private readonly ObjectPool<ElementView> elementPool;
        private readonly NotificationService notificationService;


        public HoleController(HoleModel model, HoleView view, NotificationService notificationService) : base(model,
            view)
        {
            this.notificationService = notificationService;
        }

        public bool IsInsideZone(Vector3 screenPosition)
        {
            return Model != null && Model.IsInsideEllipse(screenPosition);
        }

        public bool TryDropElement(ElementModel elementModel, ElementView elementView, Vector3 worlPosition)
        {
            if (elementView == null) return false;

            var pos = elementView.transform.position;

            PrimeTween.Tween.Position(elementView.transform, pos + Vector3.down * 1f, 0.3f, PrimeTween.Ease.InQuad)
                .Chain(PrimeTween.Tween.Alpha(elementView.GetComponent<CanvasGroup>(), 0f, 0.3f,
                    PrimeTween.Ease.InQuad))
                .OnComplete(() => { elementPool.ReturnToPool(elementView); });

            _ = notificationService.ShowNotification("DropHole");

            return true;
        }
    }
}