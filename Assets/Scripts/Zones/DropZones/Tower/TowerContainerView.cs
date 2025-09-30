using Core;
using PrimeTween;
using UnityEngine;
using System.Collections.Generic;
using Zones.DropZones.Tower.TowerElement;

namespace Zones.DropZones.Tower
{
    public class TowerContainerView : View
    {
        [SerializeField] private Transform elementsContainer;
        public Transform ElementsContainer => elementsContainer;
    }
}