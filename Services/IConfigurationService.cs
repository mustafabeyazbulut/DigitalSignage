using DigitalSignage.Models;

namespace DigitalSignage.Services
{
    public interface IConfigurationService
    {
        Task<CompanyConfiguration> GetConfigAsync(int companyId);
        Task<CompanyConfiguration> UpdateConfigAsync(int companyId, CompanyConfiguration config);
        Task<T> GetSettingAsync<T>(int companyId, string settingKey);
        Task SetSettingAsync(int companyId, string settingKey, object value);
        Task ClearCacheAsync(int companyId);
    }
}
