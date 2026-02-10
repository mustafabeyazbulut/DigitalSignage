# Multi-Tenant Company System & Dynamic Configuration

## Genel Bakış

Digital Signage, **multi-tenant** mimarisinde tasarlanmıştır. Her şirketin kendi veri, konfigürasyon ve ayarları tamamen izole olarak çalışır. **Sistem** seviyesi kaldırılmış, doğrudan Company → Department yapısı kullanılır.

---

## Yeni Mimari (v2.0)

### Eski Yapı
```
Company
├── System (KALDIRALDI)
│   └── Department
```

### Yeni Yapı (Basit ve Esnek)
```
Company ───────────────────── Multi-Tenant
├── Configuration (Dinamik)
│   ├── EmailSettings
│   ├── NotificationSettings
│   ├── LayoutDefaults
│   ├── ScheduleRules
│   └── CustomCSS
│
├── Department
│   ├── Page
│   │   ├── Layout (X-Y Grid)
│   │   │   └── Section
│   │   │       └── Content
│   │   └── Schedule
│   └── User (Role ile)
│
└── User (CompanyRole ile)
    ├── CompanyAdmin
    ├── DepartmentManager
    └── Viewer
```

---

## Company Entity

```csharp
public class Company
{
    public int CompanyID { get; set; }
    public string CompanyName { get; set; }
    public string CompanyCode { get; set; }
    public string EmailDomain { get; set; }  // Office 365 email domain
    public string LogoUrl { get; set; }
    public string PrimaryColor { get; set; }  // Dinamik tema
    public string SecondaryColor { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string ModifiedBy { get; set; }

    // Navigation
    public ICollection<Department> Departments { get; set; }
    public ICollection<Layout> Layouts { get; set; }
    public ICollection<UserCompanyRole> UserCompanyRoles { get; set; }
    public CompanyConfiguration Configuration { get; set; }
}
```

---

## Dinamik Configuration System

### CompanyConfiguration Entity

```csharp
public class CompanyConfiguration
{
    public int ConfigurationID { get; set; }
    public int CompanyID { get; set; }

    // Email Settings
    public string EmailSmtpServer { get; set; }
    public int EmailSmtpPort { get; set; }
    public string EmailFrom { get; set; }
    public string EmailUsername { get; set; }
    public string EmailPassword { get; set; }  // Encrypted
    public bool EmailNotificationsEnabled { get; set; }

    // Notification Settings
    public bool NotifyOnScheduleChange { get; set; }
    public bool NotifyOnContentChange { get; set; }
    public bool NotifyOnError { get; set; }
    public string NotificationEmail { get; set; }

    // Layout Defaults
    public int DefaultGridColumnsX { get; set; } = 2;
    public int DefaultGridRowsY { get; set; } = 2;
    public string DefaultSectionPadding { get; set; } = "10px";
    public string DefaultSectionBorder { get; set; } = "1px solid #ddd";

    // Schedule Rules
    public int MaxSchedulesPerPage { get; set; } = 10;
    public int DefaultScheduleDuration { get; set; } = 30;  // seconds
    public bool AllowRecurringSchedules { get; set; } = true;

    // Display Settings
    public int ScreenRefreshInterval { get; set; } = 5;  // seconds
    public bool EnableAutoRotation { get; set; } = true;
    public string CustomCSS { get; set; }  // HTML <style> içeriği

    // Feature Flags
    public bool EnableAnalytics { get; set; } = true;
    public bool EnableAdvancedScheduling { get; set; } = true;
    public bool EnableMediaUpload { get; set; } = true;
    public int MaxMediaSizeGB { get; set; } = 10;

    // Timestamp
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Navigation
    public Company Company { get; set; }
}
```

### Configuration Settings JSON Example

```json
{
  "companyId": 1,
  "emailSettings": {
    "smtpServer": "smtp.gmail.com",
    "smtpPort": 587,
    "emailFrom": "notifications@company.com",
    "notificationsEnabled": true
  },
  "layoutDefaults": {
    "defaultGridColumnsX": 2,
    "defaultGridRowsY": 2,
    "sectionPadding": "10px",
    "sectionBorder": "1px solid #ddd"
  },
  "scheduleRules": {
    "maxSchedulesPerPage": 10,
    "defaultDuration": 30,
    "allowRecurring": true
  },
  "displaySettings": {
    "screenRefreshInterval": 5,
    "enableAutoRotation": true,
    "customCss": "body { background-color: #f0f0f0; }"
  },
  "featureFlags": {
    "enableAnalytics": true,
    "enableAdvancedScheduling": true,
    "enableMediaUpload": true,
    "maxMediaSizeGB": 10
  }
}
```

---

## UserCompanyRole Entity

```csharp
public class UserCompanyRole
{
    public int UserCompanyRoleID { get; set; }
    public int UserID { get; set; }
    public int CompanyID { get; set; }
    public string Role { get; set; }  // CompanyAdmin, DepartmentManager, ContentEditor, Viewer
    public bool IsActive { get; set; }
    public DateTime AssignedDate { get; set; }
    public string AssignedBy { get; set; }

    // Navigation
    public User User { get; set; }
    public Company Company { get; set; }
}
```

### Roller Tanımı

| Role | Yetkiler |
|------|----------|
| **SystemAdmin** | Tüm platform yönetimi, tüm şirketler |
| **CompanyAdmin** | Şirket yönetimi, config, tüm departmanlar |
| **DepartmentManager** | Departman yönetimi, sayfa/içerik |
| **ContentEditor** | İçerik oluştur/düzenle |
| **Viewer** | Sadece okuma |

---

## Tenant Context

### ITenantContext Interface

```csharp
public interface ITenantContext
{
    int CurrentCompanyId { get; }
    int CurrentUserId { get; }

    Task<bool> HasAccessToCompanyAsync(int companyId);
    Task<bool> IsCompanyAdminAsync(int companyId);
    Task<bool> IsDepartmentManagerAsync(int departmentId);
    Task<CompanyConfiguration> GetCompanyConfigAsync(int companyId);
}
```

### TenantContext Implementation

```csharp
public class TenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserCompanyRoleRepository _userCompanyRoleRepository;
    private readonly ICompanyConfigurationRepository _configRepository;
    private readonly IMemoryCache _cache;

    public TenantContext(
        IHttpContextAccessor httpContextAccessor,
        IUserCompanyRoleRepository userCompanyRoleRepository,
        ICompanyConfigurationRepository configRepository,
        IMemoryCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _userCompanyRoleRepository = userCompanyRoleRepository;
        _configRepository = configRepository;
        _cache = cache;
    }

    public int CurrentCompanyId
    {
        get
        {
            if (_httpContextAccessor.HttpContext?.Items["CompanyId"] is int companyId)
                return companyId;

            var companyIdSession = _httpContextAccessor.HttpContext?.Session.GetInt32("SelectedCompanyId");
            return companyIdSession ?? 0;
        }
    }

    public int CurrentUserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }

    public async Task<bool> HasAccessToCompanyAsync(int companyId)
    {
        if (CurrentUserId == 0)
            return false;

        var role = await _userCompanyRoleRepository.FirstOrDefaultAsync(ucr =>
            ucr.UserID == CurrentUserId &&
            ucr.CompanyID == companyId &&
            ucr.IsActive
        );

        return role != null;
    }

    public async Task<bool> IsCompanyAdminAsync(int companyId)
    {
        if (CurrentUserId == 0)
            return false;

        var role = await _userCompanyRoleRepository.FirstOrDefaultAsync(ucr =>
            ucr.UserID == CurrentUserId &&
            ucr.CompanyID == companyId &&
            ucr.Role == "CompanyAdmin" &&
            ucr.IsActive
        );

        return role != null;
    }

    public async Task<bool> IsDepartmentManagerAsync(int departmentId)
    {
        // Implementation
        return true;
    }

    public async Task<CompanyConfiguration> GetCompanyConfigAsync(int companyId)
    {
        var cacheKey = $"company_config_{companyId}";

        if (_cache.TryGetValue(cacheKey, out CompanyConfiguration config))
            return config;

        config = await _configRepository.FirstOrDefaultAsync(c => c.CompanyID == companyId);

        if (config != null)
        {
            _cache.Set(cacheKey, config, TimeSpan.FromHours(1));
        }

        return config;
    }
}
```

---

## Configuration Service

### IConfigurationService

```csharp
public interface IConfigurationService
{
    Task<CompanyConfiguration> GetConfigAsync(int companyId);
    Task<CompanyConfiguration> UpdateConfigAsync(int companyId, CompanyConfiguration config);
    Task<T> GetSettingAsync<T>(int companyId, string settingKey);
    Task SetSettingAsync(int companyId, string settingKey, object value);
    Task ClearCacheAsync(int companyId);
}
```

### ConfigurationService Implementation

```csharp
public class ConfigurationService : IConfigurationService
{
    private readonly ICompanyConfigurationRepository _configRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ConfigurationService> _logger;

    public async Task<CompanyConfiguration> GetConfigAsync(int companyId)
    {
        var cacheKey = $"config_{companyId}";

        if (_cache.TryGetValue(cacheKey, out CompanyConfiguration config))
            return config;

        config = await _configRepository.FirstOrDefaultAsync(c => c.CompanyID == companyId);

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
                MaxMediaSizeGB = 10
            };

            config = await _configRepository.AddAsync(config);
        }

        _cache.Set(cacheKey, config, TimeSpan.FromHours(1));
        return config;
    }

    public async Task<CompanyConfiguration> UpdateConfigAsync(int companyId, CompanyConfiguration config)
    {
        config.ModifiedDate = DateTime.UtcNow;
        await _configRepository.UpdateAsync(config);

        // Clear cache
        await ClearCacheAsync(companyId);

        _logger.LogInformation($"Configuration updated for company {companyId}");

        return config;
    }

    public async Task<T> GetSettingAsync<T>(int companyId, string settingKey)
    {
        var config = await GetConfigAsync(companyId);

        var property = typeof(CompanyConfiguration).GetProperty(settingKey,
            System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);

        if (property == null)
            throw new ArgumentException($"Setting '{settingKey}' not found");

        var value = property.GetValue(config);
        return (T)Convert.ChangeType(value, typeof(T));
    }

    public async Task SetSettingAsync(int companyId, string settingKey, object value)
    {
        var config = await GetConfigAsync(companyId);

        var property = typeof(CompanyConfiguration).GetProperty(settingKey,
            System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public);

        if (property == null)
            throw new ArgumentException($"Setting '{settingKey}' not found");

        property.SetValue(config, Convert.ChangeType(value, property.PropertyType));

        await UpdateConfigAsync(companyId, config);
    }

    public async Task ClearCacheAsync(int companyId)
    {
        var cacheKey = $"config_{companyId}";
        _cache.Remove(cacheKey);
        await Task.CompletedTask;
    }
}
```

---

## Middleware: Tenant Resolver

```csharp
public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // URL'den company ID al
        var companyIdRoute = context.GetRouteValue("companyId")?.ToString();

        if (companyIdRoute != null && int.TryParse(companyIdRoute, out var companyId))
        {
            context.Items["CompanyId"] = companyId;
        }

        // Veya session'dan al
        var sessionCompanyId = context.Session.GetInt32("SelectedCompanyId");
        if (sessionCompanyId.HasValue && context.Items["CompanyId"] == null)
        {
            context.Items["CompanyId"] = sessionCompanyId.Value;
        }

        await _next(context);
    }
}

// Program.cs'de
app.UseMiddleware<TenantResolverMiddleware>();
```

---

## Company Controller

```csharp
[Route("api/companies/{companyId}")]
[ApiController]
[Authorize]
public class CompanyController : BaseController
{
    private readonly ICompanyService _companyService;
    private readonly IConfigurationService _configService;
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Şirket detaylarını al
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCompany(int companyId)
    {
        if (!await _tenantContext.HasAccessToCompanyAsync(companyId))
            return Forbid();

        var company = await _companyService.GetByIdAsync(companyId);
        return Ok(company);
    }

    /// <summary>
    /// Şirket konfigürasyonunu al
    /// </summary>
    [HttpGet("configuration")]
    public async Task<IActionResult> GetConfiguration(int companyId)
    {
        if (!await _tenantContext.HasAccessToCompanyAsync(companyId))
            return Forbid();

        var config = await _configService.GetConfigAsync(companyId);
        return Ok(config);
    }

    /// <summary>
    /// Şirket konfigürasyonunu güncelle
    /// </summary>
    [HttpPut("configuration")]
    [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> UpdateConfiguration(
        int companyId,
        [FromBody] CompanyConfigurationDTO dto)
    {
        if (!await _tenantContext.IsCompanyAdminAsync(companyId))
            return Forbid();

        var config = await _configService.GetConfigAsync(companyId);

        // Update properties
        config.DefaultGridColumnsX = dto.DefaultGridColumnsX;
        config.DefaultGridRowsY = dto.DefaultGridRowsY;
        config.ScreenRefreshInterval = dto.ScreenRefreshInterval;
        config.CustomCSS = dto.CustomCSS;
        // ... diğer properties

        var updated = await _configService.UpdateConfigAsync(companyId, config);
        return Ok(updated);
    }

    /// <summary>
    /// Özel ayarı al
    /// </summary>
    [HttpGet("setting/{settingKey}")]
    public async Task<IActionResult> GetSetting(int companyId, string settingKey)
    {
        if (!await _tenantContext.HasAccessToCompanyAsync(companyId))
            return Forbid();

        var value = await _configService.GetSettingAsync<object>(companyId, settingKey);
        return Ok(new { key = settingKey, value });
    }

    /// <summary>
    /// Özel ayarı güncelle
    /// </summary>
    [HttpPut("setting/{settingKey}")]
    [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> SetSetting(
        int companyId,
        string settingKey,
        [FromBody] object value)
    {
        if (!await _tenantContext.IsCompanyAdminAsync(companyId))
            return Forbid();

        await _configService.SetSettingAsync(companyId, settingKey, value);
        return Ok();
    }
}
```

---

## Best Practices

### 1. Her İşlemde Tenant Check
```csharp
if (!await _tenantContext.HasAccessToCompanyAsync(companyId))
    throw new UnauthorizedAccessException("Access denied");
```

### 2. Configuration Caching
```csharp
// 1 saatlik cache
_cache.Set(cacheKey, config, TimeSpan.FromHours(1));
```

### 3. Audit Logging
```csharp
_logger.LogInformation($"Configuration changed for company {companyId}");
```

### 4. Data Isolation
```csharp
// Her query'de company filter
var pages = await _pageRepository.FindAsync(p =>
    p.Department.CompanyID == _tenantContext.CurrentCompanyId
);
```

---

## Data Seeding

```csharp
public class CompanySeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (context.Companies.Any())
            return;

        var company = new Company
        {
            CompanyName = "Örnek Şirket",
            CompanyCode = "EXAMPLE",
            EmailDomain = "example.com",
            LogoUrl = "/images/logo.png",
            PrimaryColor = "#0078D4",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync();

        var config = new CompanyConfiguration
        {
            CompanyID = company.CompanyID,
            DefaultGridColumnsX = 2,
            DefaultGridRowsY = 2,
            MaxSchedulesPerPage = 10,
            DefaultScheduleDuration = 30
        };

        context.CompanyConfigurations.Add(config);
        await context.SaveChangesAsync();
    }
}
```

---

## References

- [Multi-Tenant .NET Applications](https://docs.microsoft.com/en-us/dotnet/architecture/multitenant/)
- [ASP.NET Core Data Isolation](https://learn.microsoft.com/en-us/azure/architecture/multitenant/patterns/data-isolation)
