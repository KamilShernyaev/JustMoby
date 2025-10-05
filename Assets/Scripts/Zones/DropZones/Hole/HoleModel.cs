using Core.MVC;
using UnityEngine;

namespace Zones.DropZones.Hole
{
    public class HoleModel : IModel
    {
        public Vector2 EllipseSize;

        public bool IsPointInsideEllipse(Vector2 localPoint)
        {
            var halfSize = EllipseSize / 2f;
            if (halfSize.x <= 0 || halfSize.y <= 0) return false;

            var normX = localPoint.x / halfSize.x;
            var normY = localPoint.y / halfSize.y;

            return (normX * normX + normY * normY) <= 1f;
        }
    }
}