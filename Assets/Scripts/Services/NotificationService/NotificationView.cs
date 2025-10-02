using Core;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace Services.NotificationService
{
    public class NotificationView : View
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private CanvasGroup canvasGroup;
        private Sequence showTween;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (textComponent == null)
                textComponent = GetComponentInChildren<TMP_Text>();
            gameObject.SetActive(false);
        }

        public void Show(string message, float displayDuration = 2f, float fadeDuration = 1f)
        {
            gameObject.SetActive(false);
            if (showTween.isAlive)
            {
                showTween.Stop();
            }

            textComponent.text = message;
            if (canvasGroup.alpha != 0f)
            {
                canvasGroup.alpha = 0f;
            }

            gameObject.SetActive(true);

            showTween = Tween.Alpha(canvasGroup, 1f, 0.3f, Ease.OutQuad)
                .Chain(Tween.Delay(displayDuration))
                .Chain(Tween.Alpha(canvasGroup, 0f, fadeDuration, Ease.InQuad))
                .OnComplete(() => { gameObject.SetActive(false); });
        }
    }
}