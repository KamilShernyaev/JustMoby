using Element;
using Zones.DropZones.Tower;

namespace Zones.DropZones.DropRules
{
    public interface IDropRule
    {
        bool CanAddElement(ElementModel element, TowerContainerModel towerContainer);
    }
}