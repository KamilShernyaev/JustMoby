using Core;
using Services.DragService;
using UnityEngine.EventSystems;

namespace Zones.ScrollArea.ScrollElement
{
    public class ScrollElementController : Controller<ScrollElementModel, ScrollElementView>
    {
        private IDragStartHandler dragStartHandler;

        public ScrollElementController(ScrollElementModel model, ScrollElementView view) : base(model, view)
        {
        }

        public void Initialize(IDragStartHandler dragStartHandler)
        {
            this.dragStartHandler = dragStartHandler;
            View.Initialize(this);
            View.SetSprite(Model.ElementType.Sprite);
        }

        public void OnDragStart(PointerEventData eventData)
        {
            dragStartHandler?.OnDragStart(Model, View, eventData);
            View.SetTransparency(0.5f);
        }
    }
}