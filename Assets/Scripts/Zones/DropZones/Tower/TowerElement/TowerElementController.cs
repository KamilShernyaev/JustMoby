using PrimeTween;
using Services.DragService;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Zones.DropZones.Tower.TowerElement
{
    public class TowerElementController
    {
        public TowerElementModel Model { get; }
        public TowerElementView View { get; }
        private IDragStartHandler dragStartHandler;

        public TowerElementController(TowerElementModel model, TowerElementView view)
        {
            Model = model;
            View = view;
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