using System.Collections.Generic;
using System.Linq;
using Core.MVC;
using Element;
using Services.SaveLoadService;
using UnityEngine;
using Zones.DropZones.Tower.TowerElement;

namespace Zones.DropZones.Tower
{
    public class TowerContainerModel : IModel
    {
        private readonly List<TowerElementModel> elements = new();

        public IReadOnlyList<TowerElementModel> Elements => elements;
        public Vector2 BasePosition { get; set; }
        public float CurrentHeight => elements.Sum(element => element.ElementHeight);
        public int ElementCount => elements.Count;

        public void AddElement(TowerElementModel element)
        {
            element.Index = elements.Count;
            elements.Add(element);
        }

        public void RemoveElementAt(int index)
        {
            elements.RemoveAt(index);
            for (var i = 0; i < elements.Count; i++)
            {
                elements[i].Index = i;
            }
        }

        public TowerElementModel GetElementAt(int index) => elements[index];

        public void SetElementAt(int index, TowerElementModel element) => elements[index] = element;

        public Vector2 GetTopPosition(float pivotY)
        {
            if (Elements.Count == 0)
                return BasePosition;
            return GetElementPosition(Elements.Count, pivotY);
        }

        public Vector3 GetElementPosition(int index, float pivotY = 0.5f)
        {
            var y = BasePosition.y;
            var x = BasePosition.x + elements[index].HorizontalOffset;

            for (var i = 0; i < index; i++)
            {
                y += elements[i].ElementHeight;
            }

            var offset = elements[index].ElementHeight * pivotY;
            var posY = y + offset;

            return new Vector3(x, posY, 0);
        }

        public bool CanAddElement(float newElementHeight, float availableHeight)
        {
            if (elements.Count == 0)
                return true;

            return (availableHeight - CurrentHeight) >= newElementHeight;
        }

        public TowerSaveData ToSaveData()
        {
            var data = new TowerSaveData();
            foreach (var element in Elements)
            {
                data.Elements.Add(new TowerSaveData.SavedTowerElement
                {
                    ElementTypeID = element.ElementType.ID,
                    HorizontalOffset = element.HorizontalOffset,
                    Index = element.Index,
                    ElementHeight = element.ElementHeight
                });
            }

            data.BasePosition = new SerializableVector3(BasePosition);

            return data;
        }

        public void LoadFromSaveData(TowerSaveData data, ElementType[] availableTypes)
        {
            elements.Clear();

            foreach (var savedElement in data.Elements)
            {
                var type = availableTypes.FirstOrDefault(t => t.ID == savedElement.ElementTypeID);
                if (string.IsNullOrEmpty(type.ID))
                {
                    Debug.LogWarning(
                        $"ElementType with ID {savedElement.ElementTypeID} not found in available types. Пропускаем элемент.");
                    continue;
                }

                var element = new TowerElementModel
                {
                    ElementType = type,
                    HorizontalOffset = savedElement.HorizontalOffset,
                    Index = savedElement.Index,
                    ElementHeight = savedElement.ElementHeight
                };

                elements.Add(element);
            }

            BasePosition = data.BasePosition.ToVector3();
        }
    }
}