using System.Threading.Tasks;

namespace Services.LocalizationService
{
    public interface ILocalizationService
    {
        Task<string> GetStringAsync(string key);
    }
}