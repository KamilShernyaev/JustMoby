using System.Collections.Generic;
using Core;
using Element;
using UnityEngine;
using Zones.DropZones.Tower.TowerElement;

public class TowerContainerModel : IModel
{
    private readonly List<TowerElementModel> elements = new();
    public IReadOnlyList<TowerElementModel> Elements => elements;
    public Vector3? BasePosition;
    public float MaxHorizontalOffset { get; private set; } = 50f;
    public int CurrentHeight => elements.Count;
    public float ElementHeight { get; set; } = 100f;

    public void AddElement(ElementModel element)
    {
        var horizontalOffset = Random.Range(-MaxHorizontalOffset, MaxHorizontalOffset);

        var towerElement = new TowerElementModel
        {
            ElementType = element.ElementType,
            HorizontalOffset = horizontalOffset,
            IndexInTower = elements.Count
        };

        elements.Add(towerElement);
    }

    public bool TryRemoveElementAtIndex(int index)
    {
        if (index < 0 || index >= elements.Count) return false;

        elements.RemoveAt(index);

        for (int i = index; i < elements.Count; i++)
        {
            var e = elements[i];
            e.IndexInTower = i;
            elements[i] = e;
        }

        return true;
    }

    public void RemoveElement(ElementModel element)
    {
        int index = elements.FindIndex(e => e == element);
        if (index >= 0)
            TryRemoveElementAtIndex(index);
    }

    public bool CanAddElement(float screenHeightLimit)
    {
        float towerHeight = CurrentHeight * ElementHeight;
        return (towerHeight + ElementHeight) <= screenHeightLimit;
    }
}