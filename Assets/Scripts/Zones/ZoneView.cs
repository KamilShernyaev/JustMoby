using Core.MVC;
using UnityEngine;
using UnityEngine.UI;

namespace Zones
{
    public class ZoneView : View
    {
        [SerializeField] private Image backgroundImage;

        public void SetBackground(Sprite sprite)
        {
            if (backgroundImage != null)
            {
                backgroundImage.sprite = sprite;
                backgroundImage.color = sprite != null ? Color.white : Color.clear;
            }
            else
            {
                Debug.LogWarning("BackgroundImage not assigned in ScrollContainerView prefab.");
            }
        }
    }
}