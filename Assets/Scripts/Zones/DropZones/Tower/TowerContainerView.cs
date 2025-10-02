using Core;
using Core.MVC;
using UnityEngine;

namespace Zones.DropZones.Tower
{
    public class TowerContainerView : View
    {
        [SerializeField] private Transform elementsContainer;
        public Transform ElementsContainer => elementsContainer;
    }
}