using Element;
using UnityEngine.EventSystems;

namespace Services.DragService
{
    public interface IDragStartHandler
    {
        void OnDragStart(ElementModel elementModel, ElementView elementView, PointerEventData eventData);
    }
}