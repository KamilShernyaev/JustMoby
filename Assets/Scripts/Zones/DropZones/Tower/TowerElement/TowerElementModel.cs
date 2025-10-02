using Element;
using UnityEngine;

namespace Zones.DropZones.Tower.TowerElement
{
    public class TowerElementModel : ElementModel
    {
        public float HorizontalOffset;
        public int Index;
        public float ElementHeight;

        public static TowerElementModel Create(ElementType elementType, float elementWidth)
        {
            var maxHorizontalOffset = elementWidth * 0.5f;

            return new TowerElementModel
            {
                ElementType = elementType,
                HorizontalOffset = Random.Range(-maxHorizontalOffset, maxHorizontalOffset),
                Index = -1,
                ElementHeight = 0f
            };
        }
    }
}