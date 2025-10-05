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
using Services.AnimationService;
using Services.ConfigProvider;
using VContainer;

namespace Zones.DropZones.Tower
{
    public class TowerContainerController : Controller<TowerContainerModel, TowerContainerView>, IDropZone
    {
        private IDragStartHandler dragStartHandler;
        private readonly IReadOnlyList<IDropRule> dropRules;
        private readonly INotificationService notificationService;
        private readonly ObjectPool<ElementView> elementPool;
        private readonly List<(TowerElementModel model, ElementView view)> activeElements = new();
        private readonly IAnimationService animationService;

        public TowerContainerController(IReadOnlyList<IDropRule> dropRules, TowerContainerModel model,
            TowerContainerView view, [Key("MainPool")] ObjectPool<ElementView> elementPool,
            INotificationService notificationService, IConfigProvider configProvider,
            IAnimationService animationService) : base(model, view)
        {
            this.dropRules = dropRules;
            this.elementPool = elementPool;
            this.notificationService = notificationService;
            this.animationService = animationService;
            View.SetBackground(configProvider.GetBackgroundSprite(BackgroundZoneType.Tower));
        }

        public void Initialize(IDragStartHandler dragStartHandler)
        {
            this.dragStartHandler = dragStartHandler;
            elementPool.PreWarm(20, View.ElementsContainer);
        }

        public void LoadFromSavedData()
        {
            ClearActiveElements();
            for (var i = 0; i < Model.ElementCount; i++)
            {
                var m = Model.GetElementAt(i);
                var view = elementPool.Get();
                view.SetSprite(m.ElementType.Sprite);
                view.transform.SetParent(View.ElementsContainer, false);
                view.OnBeginDragEvent += eventData => OnElementBeginDrag(m, view, eventData);
                view.OnRemoveRequested += RemoveElement;
                activeElements.Add((m, view));
            }

            UpdateElementsPositions();
        }

        private void ClearActiveElements()
        {
            foreach (var (_, v) in activeElements)
            {
                if (v != null)
                {
                    animationService.CancelAnimation(v.transform);
                    elementPool.ReturnToPool(v);
                }
            }

            activeElements.Clear();
        }

        public bool TryDropElement(ElementModel elementModel, ElementView elementView, Vector3 dropWorldPosition)
        {
            if (elementModel is TowerElementModel)
            {
                return false;
            }

            if (Model == null || View == null) return false;
            if (!dropRules.All(r => r.CanAddElement(elementModel, Model)))
            {
                return false;
            }

            var rectTransform = View.GetComponent<RectTransform>();
            if (rectTransform == null) return false;

            // Проверка высоты перед проверкой зоны
            if (Model.Elements.Count > 0) // Проверяем высоту только если башня не пуста
            {
                var zoneTopY = rectTransform.rect.yMax;
                var baseY = Model.BasePosition.y;
                var availableHeight = zoneTopY - baseY;
                var rect = elementView.GetComponent<RectTransform>();
                var newElementHeight = rect != null ? rect.rect.height * elementView.transform.localScale.y : 0f;
                if (!Model.CanAddElement(newElementHeight, availableHeight))
                {
                    Debug.Log(
                        $"Cannot add element: newElementHeight={newElementHeight}, availableHeight={availableHeight}, CurrentHeight={Model.CurrentHeight}");
                    _ = notificationService.ShowNotification("HeightLimit");
                    return false;
                }
            }

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
                    _ = notificationService.ShowNotification("MissCube");
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
            view.SetAlpha(0);
            view.transform.SetParent(View.ElementsContainer, false);
            var rect = view.GetComponent<RectTransform>();
            if (rect == null)
            {
                Debug.LogError("RectTransform not found on element view");
                elementPool.ReturnToPool(view);
                return;
            }

            var elementWidth = rect.rect.width;
            var elementHeight = rect.rect.height;
            var model = TowerElementModel.Create(elementType, elementWidth);
            model.ElementHeight = elementHeight;
            view.gameObject.SetActive(true);
            Model.AddElement(model);
            view.OnBeginDragEvent += eventData => OnElementBeginDrag(model, view, eventData);
            view.OnRemoveRequested += RemoveElement;
            activeElements.Add((model, view));
            UpdateElementsPositions();
            animationService.PlayFade(view.transform, true, 0.6f);
        }

        private void RemoveElement(ElementModel elementModel)
        {
            var index = activeElements.FindIndex(p => p.model == elementModel);
            if (index < 0) return;
            var (_, v) = activeElements[index];
            if (v == null)
            {
                Debug.LogWarning("RemoveElement: ElementView is null, skipping.");
                activeElements.RemoveAt(index);
                Model.RemoveElementAt(index);
                return;
            }

            animationService.PlayFade(v.transform, false, 0.2f, () =>
            {
                if (activeElements.Contains(((TowerElementModel model, ElementView view))(elementModel, v)))
                {
                    activeElements.RemoveAt(index);
                    Model.RemoveElementAt(index);
                    elementPool.ReturnToPool(v);
                    AnimateDropDown(index);
                }
                else
                {
                    Debug.LogWarning($"RemoveElement: ElementView {v.name} already removed or not in activeElements.");
                }
            });
        }

        private void OnElementBeginDrag(TowerElementModel model, ElementView view, PointerEventData eventData)
        {
            dragStartHandler?.OnDragStart(model, view, eventData);
        }

        private void AnimateDropDown(int startIndex)
        {
            if (startIndex >= activeElements.Count) return;
            var targets = new Transform[activeElements.Count - startIndex];
            var newPositions = new Vector3[targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                var globalIndex = startIndex + i;
                var (_, view) = activeElements[globalIndex];
                targets[i] = view.transform;
                var rt = view.GetComponent<RectTransform>();
                var pivotY = rt?.pivot.y ?? 0.5f;
                newPositions[i] = Model.GetElementPosition(globalIndex, pivotY);
            }

            animationService.PlayDropDown(targets, newPositions);
        }

        private void UpdateElementsPositions()
        {
            for (var i = 0; i < activeElements.Count; i++)
            {
                var (model, view) = activeElements[i];
                model.Index = i;
                var rt = view.GetComponent<RectTransform>();
                if (rt == null) continue;
                var pos = Model.GetElementPosition(i, rt.pivot.y);
                rt.localPosition = pos;
            }
        }
    }
}