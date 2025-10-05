using Element;
using UnityEngine;

namespace Services.DragService
{
    public interface IDropZone
    {
        bool IsInsideZone(Vector3 screenPosition);
        bool TryDropElement(ElementModel elementModel, ElementView elementView, Vector3 dropScreenPosition);
    }
}