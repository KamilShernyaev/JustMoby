using System.Threading.Tasks;
using Services.AnimationService;
using Services.LocalizationService;
using UnityEngine;
using UnityEngine.Playables;

namespace Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly NotificationView notificationView;
        private readonly ILocalizationService localizationService;
        private readonly IAnimationService animationService;

        public NotificationService(ILocalizationService localizationService, NotificationView notificationView,
            IAnimationService animationService)
        {
            this.localizationService = localizationService;
            this.notificationView = notificationView;
            this.animationService = animationService;
        }

        public async Task ShowNotification(string localizationKey, float displayDuration = 2f)
        {
            if (string.IsNullOrEmpty(localizationKey))
            {
                Debug.LogWarning("Notification key is empty");
                return;
            }

            var localizedText = await localizationService.GetStringAsync(localizationKey);
            notificationView.SetText(localizedText);
            animationService.PlayFade(notificationView.transform, true, 0.3f,
                () => { animationService.PlayFade(notificationView.transform, false, 0.3f); });
        }
    }
}