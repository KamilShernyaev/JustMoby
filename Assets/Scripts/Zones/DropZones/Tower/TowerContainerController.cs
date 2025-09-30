using System.Collections.Generic;
using System.Linq;
using Core;
using Element;
using Services.DragService;
using Services.NotificationService;
using Services.PoolService;
using UnityEngine;
using Zones.DropZones.DropRules;
using Zones.DropZones.Tower.TowerElement;

namespace Zones.DropZones.Tower
{
    public class TowerContainerController : Controller<TowerContainerModel, TowerContainerView>, IDropZone
    {
        private const float MaxHorizontalOffset = 50f;

        private readonly IReadOnlyList<IDropRule> dropRules;
        private readonly ObjectPool<TowerElementView> elementPool;
        private readonly NotificationService notificationService;
        private readonly List<TowerElementController> activeElementControllers = new();
        private IDragStartHandler dragStartHandler;

        public TowerContainerController(IReadOnlyList<IDropRule> dropRules, TowerContainerModel model,
            TowerContainerView view, ObjectPool<TowerElementView> elementPool,
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

        public void Initialize()
        {
            Model.OnModelChanged += OnModelChanged;
            UpdateView();
        }

        public bool TryDropElement(ElementModel elementModel, ElementView elementView, Vector3 dropWorldPosition)
        {
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

            if (IsFirstElement())
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
                var baseY = Model.BasePosition?.y ?? 0f;
                var availableHeight = zoneTopY - baseY;

                var rect = elementView.GetComponent<RectTransform>();
                var newElementHeight = rect != null ? rect.rect.height * elementView.transform.localScale.y : 0f;

                if (availableHeight - Model.CurrentHeight < newElementHeight)
                {
                    _ = notificationService.ShowNotification("HeightLimit");
                    return false;
                }
            }

            AddElement(elementModel, elementView);
            _ = notificationService.ShowNotification("PlaceCube");

            return true;
        }


        public bool IsInsideZone(Vector3 screenPosition)
        {
            if (View == null) return false;
            var rectTransform = View.GetComponent<RectTransform>();
            if (rectTransform == null) return false;

            if (IsFirstElement())
            {
                return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition);
            }

            var topElementController = activeElementControllers.LastOrDefault();
            if (topElementController == null) return false;
            var topRect = topElementController.View.GetComponent<RectTransform>();
            return topRect != null && RectTransformUtility.RectangleContainsScreenPoint(topRect, screenPosition);
        }

        private void AddElementInternal(ElementModel elementModel, ElementView elementView)
        {
            var rectTransform = elementView.GetComponent<RectTransform>();
            var horizontalOffset = Random.Range(rectTransform.rect.width * -0.5f, rectTransform.rect.width * 0.5f);
            var index = Model.ElementCount;

            var elementHeight = rectTransform != null
                ? rectTransform.rect.height * elementView.transform.localScale.y
                : 0f;

            var towerElement = new TowerElementModel
            {
                ElementType = elementModel.ElementType,
                HorizontalOffset = horizontalOffset,
                Index = index,
                ElementHeight = elementHeight
            };

            Model.AddElementInternal(towerElement);

            var towerElementView = elementPool.Get();

            if (Model.Elements.Count == 1 && Model.BasePosition.HasValue)
            {
                towerElementView.transform.localPosition = Model.BasePosition.Value;
            }
            else
            {
                towerElementView.transform.localPosition = rectTransform.localPosition;
            }

            var elementController = new TowerElementController(towerElement, towerElementView);
            elementController.Initialize(dragStartHandler);

            towerElementView.Initialize(elementController);

            activeElementControllers.Add(elementController);
        }


        public void AddElement(ElementModel elementModel, ElementView elementView)
        {
            AddElementInternal(elementModel, elementView);
            UpdateElementsPositions();
        }

        public bool IsFirstElement() => Model.Elements.Count == 0;

        private void UpdateElementsPositions()
        {
            foreach (var element in activeElementControllers)
            {
                var elementView = element.View;
                var rt = elementView.GetComponent<RectTransform>();

                var model = element.Model;

                if (Model.BasePosition != null)
                {
                    var y = Model.BasePosition.Value.y;
                    var x = Model.BasePosition.Value.x + model.HorizontalOffset;
                    for (var j = 0; j < model.Index; j++)
                    {
                        y += Model.Elements[j].ElementHeight;
                    }

                    var firstView = activeElementControllers[0].View;
                    var firstRt = firstView.GetComponent<RectTransform>();
                    var pivotY = firstRt.pivot.y;
                    var offset = model.ElementHeight * pivotY;

                    var posY = y - offset;

                    rt.localPosition = new Vector3(x, posY, 0);
                }
            }
        }

        private void UpdateView()
        {
            ClearElements();

            foreach (var elementModel in Model.Elements)
            {
                var elementView = elementPool.Get();
                elementView.transform.SetParent(View.ElementsContainer, false);
                elementView.gameObject.SetActive(true);

                var elementController = new TowerElementController(elementModel, elementView);
                elementController.Initialize(dragStartHandler);
                elementView.Initialize(elementController);

                activeElementControllers.Add(elementController);
            }

            UpdateElementsPositions();
        }

        private void ClearElements()
        {
            foreach (var controller in activeElementControllers.Where(controller => controller.View != null))
            {
                controller.View.gameObject.SetActive(false);
                elementPool.ReturnToPool(controller.View);
            }

            activeElementControllers.Clear();
        }

        protected override void OnModelChanged()
        {
            base.OnModelChanged();
            UpdateView();
        }
    }
}