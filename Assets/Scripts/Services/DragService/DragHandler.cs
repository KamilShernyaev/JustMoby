using Element;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace Services.DragService
{
    public class DragHandler : MonoBehaviour, IDragStartHandler
    {
        private DragController draggingController;

        [Inject]
        public void Construct(DragController draggingController)
        {
            this.draggingController = draggingController;
        }

        public void OnDragStart(ElementModel elementModel, ElementView elementView, PointerEventData eventData)
        {
            draggingController.StartDrag(elementModel, eventData);
        }
        
    }
}