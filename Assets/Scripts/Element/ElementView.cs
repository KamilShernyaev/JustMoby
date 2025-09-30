using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Element
{
    public abstract class ElementView : View
    {
        [SerializeField] protected Image image;
        [SerializeField] protected CanvasGroup canvasGroup;

        public void SetSprite(Sprite sprite)
        {
            if (image != null) image.sprite = sprite;
        }
    }
}