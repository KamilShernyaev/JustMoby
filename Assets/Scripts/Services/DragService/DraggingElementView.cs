using Element;
using UnityEngine;
using UnityEngine.EventSystems;
using PrimeTween;

namespace Services.DragService
{
    public class DraggingElementView : ElementView
    {
        private Tween fadeTween;
        private void Awake() => gameObject.SetActive(false);

        public void Show(Sprite sprite, Vector3 position)
        {
            SetSprite(sprite);
            SetPosition(position);

            canvasGroup.alpha = 0.5f;
            gameObject.SetActive(true);
        }

        public void FadeOutAndHide(float duration)
        {
            if (canvasGroup == null || canvasGroup.alpha == 0f)
            {
                Hide();
                return;
            }
            if (fadeTween.isAlive)
            {
                fadeTween.Stop();
            }
            fadeTween = Tween.Alpha(canvasGroup, 0f, duration, Ease.InQuad)
                .OnComplete(Hide);
        }

        public void Hide() => gameObject.SetActive(false);
        public void SetPosition(Vector3 position) => transform.position = position;
    }
}