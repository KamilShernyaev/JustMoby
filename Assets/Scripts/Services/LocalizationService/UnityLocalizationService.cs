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

            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                    tcs.SetResult(op.Result);
                else
                    tcs.SetResult(key);
            };

            return tcs.Task;
        }
    }
}