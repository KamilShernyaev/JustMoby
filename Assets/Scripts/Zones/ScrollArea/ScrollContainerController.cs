using Core;
using System;
using UnityEngine;
using Services.DragService;
using System.Collections.Generic;
using Element;
using Services.PoolService;
using Zones.ScrollArea.ScrollElement;

namespace Zones.ScrollArea
{
    public class ScrollContainerController : Controller<ScrollContainerModel, ScrollContainerView>
    {
        private readonly ObjectPool<ScrollElementView> elementPool;
        private IDragStartHandler dragStartHandler;
        private readonly Func<ElementType, ScrollElementView> elementFactory;
        private readonly List<ScrollElementView> activeElements = new();

        public ScrollContainerController(ScrollContainerModel model, ScrollContainerView view,
            IDragStartHandler dragStartHandler,
            ObjectPool<ScrollElementView> elementPool,
            Func<ElementType, ScrollElementView> elementFactory) : base(model, view)
        {
            this.elementPool = elementPool;
            this.elementFactory = elementFactory;
            RefreshElements();
        }

        public void Initialize(IDragStartHandler dragStartHandler)
        {
            this.dragStartHandler = dragStartHandler;
        }
        
        protected override void OnModelChanged()
        {
            base.OnViewChanged();
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
                elementView.transform.SetParent(View.transform, false);
                activeElements.Add(elementView);
            }
        }


        private void ClearElements()
        {
            if (activeElements.Count <= 0)
                return;

            foreach (var ev in activeElements)
            {
                elementPool.ReturnToPool(ev);
            }

            activeElements.Clear();
        }
    }
}