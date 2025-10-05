using System.Threading.Tasks;

namespace Services.NotificationService
{
    public interface INotificationService
    {
        Task ShowNotification(string localizationKey, float displayDuration = 2f);
    }
}