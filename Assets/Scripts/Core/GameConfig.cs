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

        public ElementType[] AvailableTypes => availableTypes;
        public int BottomElementCount => bottomCubeCount;
    }
}