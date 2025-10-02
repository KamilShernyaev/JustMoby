using System.Collections.Generic;
using Core;
using Element;
using Services.ConfigProvider;
using Zones.ScrollArea.ScrollElement;

namespace Zones.ScrollArea
{
    public class ScrollContainerModel : IModel
    {
        public readonly List<ScrollElementModel> ElementsScroll = new();

        public void InitializeElements(IConfigProvider configProvider)
        {
            ElementsScroll.Clear();
            var availableTypes = configProvider.AvailableTypes;
            var bottomElementCount = configProvider.BottomElementCount;

            if (availableTypes == null || availableTypes.Length == 0 ||
                bottomElementCount <= 0)
                return;
            
            var perTypeCount = bottomElementCount / availableTypes.Length;
            var remainder = bottomElementCount % availableTypes.Length;
            for (var i = 0; i < availableTypes.Length; i++)
            {
                var count = perTypeCount + (i < remainder ? 1 : 0);
                var type = availableTypes[i];
                for (var j = 0; j < count; j++)
                {
                    ElementsScroll.Add(new ScrollElementModel
                    {
                        ElementType = type
                    });
                }
            }
        }
    }
}