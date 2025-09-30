using System.Collections.Generic;
using Element;

namespace Services.ConfigProvider
{
    public interface IConfigProvider
    {
        ElementType[] AvailableTypes { get; }
        int BottomElementCount { get; }
    }
}