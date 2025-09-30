using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Element;
using Services.SaveLoadService;
using UnityEngine;
using Zones.DropZones.Tower.TowerElement;

namespace Zones.DropZones.Tower
{
    public class TowerContainerModel : IModel
    {
        private readonly List<TowerElementModel> elements = new();

        public event Action OnModelChanged;
        public IReadOnlyList<TowerElementModel> Elements => elements;
        public Vector3? BasePosition { get; set; }

        public float CurrentHeight => elements.Sum(element => element.ElementHeight);

        public int ElementCount => elements.Count;
        internal void AddElementInternal(TowerElementModel element) => elements.Add(element);

        internal void RemoveElementAtInternal(int index) => elements.RemoveAt(index);

        internal TowerElementModel GetElementAt(int index) => elements[index];

        internal void SetElementAt(int index, TowerElementModel element) => elements[index] = element;

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

            data.BasePosition = BasePosition.HasValue
                ? new SerializableVector3(BasePosition.Value)
                : new SerializableVector3(Vector3.zero);

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
                    Debug.LogWarning($"ElementType with ID {savedElement.ElementTypeID} not found in available types.");
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

            OnModelChanged?.Invoke();
        }
    }
}