using UnityEngine;

namespace Zones.DropZones.Tower
{
    public class TowerContainerView : ZoneView
    {
        [SerializeField] private Transform elementsContainer;
        public Transform ElementsContainer => elementsContainer;
    }
}