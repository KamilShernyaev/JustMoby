using Element;
using UnityEngine;
using PrimeTween;

namespace Services.DragService
{
    public class DraggingElementView : ElementView
    {
        private void Awake() => gameObject.SetActive(false);

        public void Show(Sprite sprite, Vector3 position)
        {
            SetSprite(sprite);
            SetPosition(position);
            SetAlpha(0.5f);
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
        public void SetPosition(Vector3 position) => transform.position = position;
    }
}