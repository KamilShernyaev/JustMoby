using Element;
using Zones.DropZones.Tower;

namespace Zones.DropZones.DropRules
{
    public class NonRestrictionTowerDropRule : IDropRule
    {
        public bool CanAddElement(ElementModel element, TowerContainerModel towerContainer) => true;
    }

    public class OnlyOneColorTowerDropRule : IDropRule
    {
        public bool CanAddElement(ElementModel element, TowerContainerModel towerContainer)
        {
            if (towerContainer.Elements.Count == 0)
                return true;

            return element.ElementType.ID == towerContainer.Elements[0].ElementType.ID;
        }
    }
}