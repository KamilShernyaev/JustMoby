using System.Linq;
using Element;
using Services.ConfigProvider;
using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "ElementsType", menuName = "Game/ElementsType", order = 1)]
    public class GameConfig : ScriptableObject, IConfigProvider
    {
        [SerializeField] private ElementType[] availableTypes;
        [SerializeField] private int bottomCubeCount = 24;

        [Header("Backgrounds")] [SerializeField]
        private BackgroundConfig[] backgrounds;

        public ElementType[] AvailableTypes => availableTypes;

        public int BottomElementCount => bottomCubeCount;

        public Sprite GetBackgroundSprite(BackgroundZoneType zoneType)
        {
            if (backgrounds == null || backgrounds.Length == 0) return null;
            var config = backgrounds.FirstOrDefault(b => b.ZoneType == zoneType);
            return config.Sprite;
        }
    }
}