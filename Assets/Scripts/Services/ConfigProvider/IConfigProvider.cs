using Core;
using Element;
using UnityEngine;

namespace Services.ConfigProvider
{
    public interface IConfigProvider
    {
        ElementType[] AvailableTypes { get; }
        int BottomElementCount { get; }
        Sprite HoleSprite { get; }
        Sprite GetBackgroundSprite(BackgroundZoneType zoneType);
    }
}