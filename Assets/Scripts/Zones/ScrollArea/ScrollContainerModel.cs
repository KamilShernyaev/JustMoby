using System.Collections.Generic;
using Core;
using Zones.ScrollArea.ScrollElement;

namespace Zones.ScrollArea
{
    public class ScrollContainerModel : IModel
    {
        public List<ScrollElementModel> ElementsScroll = new();
    }
}