using Element;
using UnityEngine.EventSystems;

namespace Zones.DropZones.Tower.TowerElement
{
    public class TowerElementView : ContainerElementView
    {
        private TowerElementController elementController;

        public void Initialize(TowerElementController controller)
        {
            elementController = controller;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            elementController?.OnDragStart(eventData);
        }
    }
}