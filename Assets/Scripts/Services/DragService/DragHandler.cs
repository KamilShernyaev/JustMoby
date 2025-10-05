using Element;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Services.DragService
{
    public class DragHandler : MonoBehaviour, IDragStartHandler
    {
        [Inject] private DragController dragController;
        [Inject] private DraggingElementModel draggingModel;

        public void OnDragStart(ElementModel elementModel, ElementView elementView, PointerEventData eventData)
        {
            dragController?.StartDrag(elementModel, elementView, eventData);
        }
    }
}