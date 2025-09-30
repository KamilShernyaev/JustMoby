using Core;
using UnityEngine;

namespace Zones.DropZones.Hole
{
    public class HoleModel : IModel
    {
        public Vector3 Position { get; set; }
        public Vector2 EllipseSize { get; set; }

        public bool IsInsideEllipse(Vector3 point)
        {
            var relative = new Vector2(point.x - Position.x, point.y - Position.y);
            return (relative.x * relative.x) / (EllipseSize.x * EllipseSize.x) +
                (relative.y * relative.y) / (EllipseSize.y * EllipseSize.y) <= 1f;
        }
    }
}