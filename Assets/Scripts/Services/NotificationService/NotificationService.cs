using System.Threading.Tasks;
using Services.LocalizationService;
using UnityEngine;

namespace Services.NotificationService
{
    public class NotificationService
    {
        private readonly ILocalizationService localizationService;
        private readonly NotificationView notificationView;

        public NotificationService(ILocalizationService localizationService, NotificationView notificationView)
        {
            this.localizationService = localizationService;
            this.notificationView = notificationView;
        }

        public async Task ShowNotification(string localizationKey, float duration = 2f)
        {
            if (string.IsNullOrEmpty(localizationKey))
            {
                Debug.LogWarning("Notification key is empty");
                return;
            }

            var localizedText = await localizationService.GetStringAsync(localizationKey);

            notificationView.Show(localizedText, duration);
        }
    }
}