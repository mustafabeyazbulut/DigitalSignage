using Microsoft.Extensions.Caching.Memory;
using DigitalSignage.Data;
using DigitalSignage.Models;
using System.Reflection;

namespace DigitalSignage.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ConfigurationService> _logger;

        public ConfigurationService(
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILogger<ConfigurationService> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CompanyConfiguration> GetConfigAsync(int companyId)
        {
            var cacheKey = $"config_{companyId}";

            if (_cache.TryGetValue(cacheKey, out CompanyConfiguration? config) && config != null)
                return config;

            config = await _unitOfWork.CompanyConfigurations.FirstOrDefaultAsync(c => c.CompanyID == companyId);

            if (config == null)
            {
                // Default configuration
                config = new CompanyConfiguration
                {
                    CompanyID = companyId,
                    DefaultGridColumnsX = 2,
                    DefaultGridRowsY = 2,
                    MaxSchedulesPerPage = 10,
                    DefaultScheduleDuration = 30,
                    ScreenRefreshInterval = 5,
                    EnableAutoRotation = true,
                    EnableAnalytics = true,
                    EnableAdvancedScheduling = true,
                    EnableMediaUpload = true,
                    MaxMediaSizeGB = 10,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.CompanyConfigurations.AddAsync(config);
                await _unitOfWork.SaveChangesAsync();
            }

            _cache.Set(cacheKey, config, TimeSpan.FromHours(1));
            return config;
        }

        public async Task<CompanyConfiguration> UpdateConfigAsync(int companyId, CompanyConfiguration config)
        {
            config.ModifiedDate = DateTime.UtcNow;
            await _unitOfWork.CompanyConfigurations.UpdateAsync(config);
            await _unitOfWork.SaveChangesAsync();

            // Clear cache
            await ClearCacheAsync(companyId);

            _logger.LogInformation($"Configuration updated for company {companyId}");

            return config;
        }

        public async Task<T> GetSettingAsync<T>(int companyId, string settingKey)
        {
            var config = await GetConfigAsync(companyId);

            var property = typeof(CompanyConfiguration).GetProperty(settingKey,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                throw new ArgumentException($"Setting '{settingKey}' not found");

            var value = property.GetValue(config);
            return (T)Convert.ChangeType(value!, typeof(T))!;
        }

        public async Task SetSettingAsync(int companyId, string settingKey, object value)
        {
            var config = await GetConfigAsync(companyId);

            var property = typeof(CompanyConfiguration).GetProperty(settingKey,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                throw new ArgumentException($"Setting '{settingKey}' not found");

            property.SetValue(config, Convert.ChangeType(value, property.PropertyType));

            await UpdateConfigAsync(companyId, config);
        }

        public Task ClearCacheAsync(int companyId)
        {
            var cacheKey = $"config_{companyId}";
            _cache.Remove(cacheKey);
            return Task.CompletedTask;
        }
    }
}
