using Core.MVC;
using TMPro;
using UnityEngine;

namespace Services.NotificationService
{
    public class NotificationView : View
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (textComponent == null) textComponent = GetComponentInChildren<TMP_Text>();
        }

        public void SetText(string message)
        {
            if (textComponent != null) textComponent.text = message;
        }
    }
}