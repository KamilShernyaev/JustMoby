using Core;
using UnityEngine;

namespace Zones.DropZones.Tower
{
    public class TowerContainerView : View
    {
        [SerializeField] private Transform elementsContainer;
        public Transform ElementsContainer => elementsContainer;
    }
}