using Element;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Zones.ScrollArea.ScrollElement
{
    public class ScrollElementView : ContainerElementView
    {
        private ScrollElementController elementController;

        public void Initialize(ScrollElementController controller)
        {
            elementController = controller;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            elementController?.OnDragStart(eventData);
        }
    }
}