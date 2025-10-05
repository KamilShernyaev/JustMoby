using UnityEngine;
using UnityEngine.UI;

namespace Zones.DropZones.Hole
{
    [RequireComponent(typeof(RectTransform))]
    public class HoleView : ZoneView
    {
        [SerializeField] private Image holeImage;
        public Image HoleImage => holeImage;
    }
}