#nullable enable
using Core;
using System;
using System.Collections.Generic;
using Services.DragService;
using Element;
using Services.ConfigProvider;
using UnityEngine.EventSystems;
using Zones.ScrollArea.ScrollElement;

namespace Zones.ScrollArea
{
    public class ScrollContainerController : Controller<ScrollContainerModel, ScrollContainerView>
    {
        private readonly IDragStartHandler? dragStartHandler;
        private readonly Func<ElementType, ElementView> elementFactory;
        private readonly IConfigProvider configProvider;
        private readonly List<(ScrollElementModel model, ElementView view)> activeElements = new();

        public ScrollContainerController(ScrollContainerModel model, ScrollContainerView view,
            IDragStartHandler? dragStartHandler, Func<ElementType, ElementView> elementFactory,
            IConfigProvider configProvider) : base(model, view)
        {
            this.dragStartHandler = dragStartHandler;
            this.elementFactory = elementFactory;
            this.configProvider = configProvider;

            if (model == null) throw new ArgumentNullException(nameof(model), "ScrollContainerModel is NULL!");
            if (view == null) throw new ArgumentNullException(nameof(view), "ScrollContainerView is NULL!");
            if (elementFactory == null)
                throw new ArgumentNullException(nameof(elementFactory), "Func<ElementType, ElementView> is NULL!");
            if (configProvider == null)
                throw new ArgumentNullException(nameof(configProvider), "IConfigProvider is NULL!");

            Model.InitializeElements(configProvider);
            RefreshElements();

            View.SetBackground(configProvider.GetBackgroundSprite(BackgroundZoneType.Scroll));
        }

        protected override void OnModelChanged()
        {
            base.OnModelChanged();
            RefreshElements();
        }

        private void RefreshElements()
        {
            if (Model?.ElementsScroll == null || View == null)
            {
                return;
            }

            ClearElements();

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
            foreach ((_, var view) in activeElements)
            {
                if (view != null && view.gameObject != null)
                {
                    UnityEngine.Object.Destroy(view.gameObject);
                }
            }

            activeElements.Clear();
        }

        private void OnElementBeginDrag(ScrollElementModel model, ElementView view, PointerEventData eventData)
        {
            dragStartHandler?.OnDragStart(model, view, eventData);
        }
    }
}