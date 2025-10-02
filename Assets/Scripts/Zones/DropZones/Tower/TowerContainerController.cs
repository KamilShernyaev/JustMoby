using System.Collections.Generic;
using System.Linq;
using Core;
using Element;
using Services.DragService;
using Services.NotificationService;
using Services.PoolService;
using UnityEngine;
using UnityEngine.EventSystems;
using Zones.DropZones.DropRules;
using Zones.DropZones.Tower.TowerElement;
using PrimeTween;

namespace Zones.DropZones.Tower
{
    public class TowerContainerController : Controller<TowerContainerModel, TowerContainerView>, IDropZone
    {
        private IDragStartHandler dragStartHandler;
        private readonly IReadOnlyList<IDropRule> dropRules;
        private readonly NotificationService notificationService;
        private readonly ObjectPool<ElementView> elementPool;
        private readonly List<(TowerElementModel model, ElementView view)> activeElements = new();

        public TowerContainerController(IReadOnlyList<IDropRule> dropRules, TowerContainerModel model,
            TowerContainerView view, ObjectPool<ElementView> elementPool,
            NotificationService notificationService) : base(model, view)
        {
            this.dropRules = dropRules;
            this.elementPool = elementPool;
            this.notificationService = notificationService;
        }

        public void Initialize(IDragStartHandler dragStartHandler)
        {
            this.dragStartHandler = dragStartHandler;
            elementPool.PreWarm(20, View.ElementsContainer);
        }

        public void LoadFromSavedData()
        {
            ClearActiveElements();
            for (int i = 0; i < Model.ElementCount; i++)
            {
                var m = Model.GetElementAt(i);
                var view = elementPool.Get();
                view.SetSprite(m.ElementType.Sprite);
                view.transform.SetParent(View.ElementsContainer, false);
                view.OnBeginDragEvent += eventData => OnElementBeginDrag(m, view, eventData);
                view.OnDragEvent += eventData => OnElementDrag(m, view, eventData);
                view.OnEndDragEvent += eventData => OnElementEndDrag(m, view, eventData);
                view.OnRemoveRequested += RemoveElement;
                activeElements.Add((m, view));
            }
            UpdateElementsPositions();
        }

        private void ClearActiveElements()
        {
            foreach (var (_, v) in activeElements)
            {
                elementPool.ReturnToPool(v);
            }
            activeElements.Clear();
        }

        public bool TryDropElement(ElementModel elementModel, ElementView elementView, Vector3 dropWorldPosition)
        {
            if (elementModel is DraggingElementModel { OriginalModel: TowerElementModel })
            {
                return true;
            }

            if (Model == null || View == null) return false;
            if (!dropRules.All(r => r.CanAddElement(elementModel, Model)))
            {
                return false;
            }

            var rectTransform = View.GetComponent<RectTransform>();
            if (rectTransform == null) return false;
            if (!IsInsideZone(dropWorldPosition))
            {
                _ = notificationService.ShowNotification("MissCube");
                return false;
            }

            if (Model.Elements.Count == 0)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, dropWorldPosition, null,
                        out var localPoint))
                {
                    var rect = rectTransform.rect;
                    var clampedX = Mathf.Clamp(localPoint.x, rect.xMin, rect.xMax);
                    var clampedY = Mathf.Clamp(localPoint.y, rect.yMin, rect.yMax);
                    Model.BasePosition = new Vector2(clampedX, clampedY);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                var zoneTopY = rectTransform.rect.yMax;
                var baseY = Model.BasePosition.y;
                var availableHeight = zoneTopY - baseY;
                var rect = elementView.GetComponent<RectTransform>();
                var newElementHeight = rect != null ? rect.rect.height * elementView.transform.localScale.y : 0f;
                if (!Model.CanAddElement(newElementHeight, availableHeight))
                {
                    _ = notificationService.ShowNotification("HeightLimit");
                    return false;
                }
            }

            AddElementInternal(elementModel.ElementType);
            _ = notificationService.ShowNotification("PlaceCube");
            return true;
        }

        public bool IsInsideZone(Vector3 screenPosition)
        {
            if (View == null) return false;
            var rectTransform = View.GetComponent<RectTransform>();
            if (rectTransform == null) return false;
            if (Model.Elements.Count == 0)
            {
                return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition);
            }

            if (activeElements.Count == 0) return false;
            var topView = activeElements.Last().view;
            var topRect = topView.GetComponent<RectTransform>();
            return topRect != null && RectTransformUtility.RectangleContainsScreenPoint(topRect, screenPosition);
        }

        private void AddElementInternal(ElementType elementType)
        {
            var view = elementPool.Get();
            view.SetSprite(elementType.Sprite);
            view.transform.SetParent(View.ElementsContainer, false);
            var rect = view.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("RectTransform not found on element view");
                elementPool.ReturnToPool(view);
                return;
            }

            var elementWidth = rect.rect.width * view.transform.localScale.x;
            var elementHeight = rect.rect.height * view.transform.localScale.y;
            
            var model = TowerElementModel.Create(elementType, elementWidth);
            model.ElementHeight = elementHeight;
            Model.AddElement(model);
            
            view.OnBeginDragEvent += eventData => OnElementBeginDrag(model, view, eventData);
            view.OnDragEvent += eventData => OnElementDrag(model, view, eventData);
            view.OnEndDragEvent += eventData => OnElementEndDrag(model, view, eventData);
            view.OnRemoveRequested += RemoveElement;
            
            activeElements.Add((model, view));
            UpdateElementsPositions();
        }

        private void RemoveElement(ElementModel elementModel)
        {
            var index = activeElements.FindIndex(p => p.model == elementModel);
            if (index < 0) return;

            (_, var v) = activeElements[index];
            activeElements.RemoveAt(index);
            Model.RemoveElementAt(index);
            elementPool.ReturnToPool(v);

            AnimateDropDown(index);
        }

        private void OnElementBeginDrag(TowerElementModel model, ElementView view, PointerEventData eventData)
        {
            dragStartHandler?.OnDragStart(model, view, eventData);
        }

        private void OnElementEndDrag(TowerElementModel model, ElementView view, PointerEventData eventData)
        {
        }

        private void OnElementDrag(TowerElementModel model, ElementView view, PointerEventData eventData)
        {
        }

        private void AnimateDropDown(int startIndex)
        {
            const float duration = 0.5f;
            for (var i = startIndex; i < activeElements.Count; i++)
            {
                var (_, view) = activeElements[i];
                var rt = view.GetComponent<RectTransform>();
                if (rt == null) continue;
                var newPos = Model.GetElementPosition(i, rt.pivot.y);
                Tween.LocalPosition(rt, newPos, duration, Ease.InOutQuad);
            }
        }

        private void UpdateElementsPositions()
        {
            for (var i = 0; i < activeElements.Count; i++)
            {
                (var model, var view) = activeElements[i];
                model.Index = i;
                var rt = view.GetComponent<RectTransform>();
                if (rt == null) continue;
                var pos = Model.GetElementPosition(i, rt.pivot.y);
                rt.localPosition = pos;
            }
        }
    }
}