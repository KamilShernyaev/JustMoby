using Core;
using System;
using Services.DragService;
using System.Collections.Generic;
using Element;
using Services.ConfigProvider;
using Services.PoolService;
using UnityEngine.EventSystems;
using Zones.ScrollArea.ScrollElement;

namespace Zones.ScrollArea
{
    public class ScrollContainerController : Controller<ScrollContainerModel, ScrollContainerView>
    {
        private readonly IDragStartHandler dragStartHandler;
        private readonly Func<ElementType, ElementView> elementFactory;
        private readonly List<(ScrollElementModel model, ElementView view)> activeElements = new();

        public ScrollContainerController(ScrollContainerModel model, ScrollContainerView view,
            IDragStartHandler dragStartHandler, Func<ElementType, ElementView> elementFactory,
            IConfigProvider configProvider) : base(model, view)
        {
            this.elementFactory = elementFactory;
            this.dragStartHandler = dragStartHandler;
            Model.InitializeElements(configProvider);
            RefreshElements();
        }

        protected override void OnModelChanged()
        {
            base.OnModelChanged();
            RefreshElements();
        }

        private void RefreshElements()
        {
            ClearElements();
            if (Model?.ElementsScroll == null || elementFactory == null)
            {
                return;
            }

            foreach (var elementModel in Model.ElementsScroll)
            {
                var elementView = elementFactory(elementModel.ElementType);
                elementView.SetSprite(elementModel.ElementType.Sprite);
                elementView.transform.SetParent(View.transform, false);
                elementView.OnBeginDragEvent += eventData => OnElementBeginDrag(elementModel, elementView, eventData);
                activeElements.Add((elementModel, elementView));
            }
        }

        private void ClearElements()
        {
            activeElements.Clear();
        }

        private void OnElementBeginDrag(ScrollElementModel model, ElementView view, PointerEventData eventData)
        {
            dragStartHandler?.OnDragStart(model, view, eventData);
        }
    }
}