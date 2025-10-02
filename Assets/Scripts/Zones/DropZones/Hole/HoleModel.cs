using Core;
using UnityEngine;

namespace Zones.DropZones.Hole
{
    public class HoleModel : IModel
    {
        public Vector3 Position { get; set; }
        public Vector2 EllipseSize { get; set; }

        public bool IsPointInsideEllipse(Vector2 point)
        {
            var center = new Vector2(Position.x, Position.y);
            var halfSize = EllipseSize / 2f;

            var delta = point - center;
            var normX = delta.x / halfSize.x;
            var normY = delta.y / halfSize.y;

            return (normX * normX + normY * normY) <= 1f;
        }
    }
}