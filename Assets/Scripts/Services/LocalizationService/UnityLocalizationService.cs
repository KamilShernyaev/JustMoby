using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Services.LocalizationService
{
    public class UnityLocalizationService : ILocalizationService
    {
        public Task<string> GetStringAsync(string key)
        {
            var tcs = new TaskCompletionSource<string>();

            var localizedString = new LocalizedString
            {
                TableReference = "NotificationTable",
                TableEntryReference = key
            };

            var handle = localizedString.GetLocalizedStringAsync();

            handle.Completed += op => { tcs.SetResult(op.Status == AsyncOperationStatus.Succeeded ? op.Result : key); };

            return tcs.Task;
        }
    }
}