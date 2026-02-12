# Office 365 Authentication & Azure AD Integration

## Genel Bakış

Digital Signage, Microsoft Office 365 ve Azure Active Directory (Azure AD) ile entegre çalışır. Kurumsal güvenlik, SSO ve MFA desteği sağlanır.

---

## Architecture

### OAuth 2.0 / OpenID Connect Flow

```
User Browser
    ↓ (Click Login)
Digital Signage App
    ↓ (Redirect to Azure AD)
Azure AD Login Page
    ↓ (User Authentication)
Azure AD
    ↓ (Issue Token)
Digital Signage App
    ↓ (Create Session)
User Dashboard
```

---

## Azure AD Setup

### 1. Application Registration

#### Azure Portal Steps:
```
1. Azure AD → App registrations → New registration
2. Name: "Digital Signage"
3. Supported account types: "Accounts in this organizational directory only"
4. Redirect URI: https://yourdomain.com/signin-oidc
5. Register
```

#### Application ID (Keep Safe)
```
- Application ID (Client ID)
- Directory ID (Tenant ID)
- Client Secret
```

### 2. Configure API Permissions

```
Required Permissions:
- Microsoft Graph
  - User.Read (Delegated)
  - User.ReadBasic.All (Delegated)
  - Mail.Read (Delegated - Optional)
  - Calendars.Read (Delegated - Optional)
```

### 3. Certificates & Secrets

```
Create Client Secret:
- Value: Your-Secret-Key (Copy immediately!)
- Expires: 1-2 years
```

---

## Implementation

### Program.cs Configuration

```csharp
var builder = WebApplication.CreateBuilder(args);

// Azure AD Authentication
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SystemAdmin", policy =>
        policy.RequireClaim("roles", "SystemAdmin"));

    options.AddPolicy("CompanyAdmin", policy =>
        policy.RequireClaim("roles", "CompanyAdmin"));

    options.AddPolicy("Manager", policy =>
        policy.RequireRole("Manager"));
});

// Session & Cookie
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAzureAdService, AzureAdService>();

var app = builder.Build();

// Middleware
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
```

### appsettings.json Configuration

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "CallbackPath": "/signin-oidc",
    "Domain": "yourdomain.onmicrosoft.com"
  },
  "MicrosoftGraph": {
    "BaseUrl": "https://graph.microsoft.com/v1.0",
    "Scopes": [
      "User.Read",
      "Mail.Read",
      "Calendars.Read"
    ]
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=..."
  }
}
```

---

## Azure AD Service

### IAzureAdService Interface

```csharp
public interface IAzureAdService
{
    Task<UserProfileDTO> GetUserProfileAsync(string accessToken);
    Task<List<string>> GetUserGroupsAsync(string accessToken);
    Task<UserEmailDTO> GetUserEmailAsync(string accessToken);
    Task<UserPhotoDTO> GetUserPhotoAsync(string accessToken);
}
```

### AzureAdService Implementation

```csharp
public class AzureAdService : IAzureAdService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureAdService> _logger;

    public AzureAdService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AzureAdService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<UserProfileDTO> GetUserProfileAsync(string accessToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_configuration["MicrosoftGraph:BaseUrl"]}/me");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var profile = JsonSerializer.Deserialize<UserProfileDTO>(content);

            _logger.LogInformation($"User profile retrieved: {profile.Mail}");
            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            throw;
        }
    }

    public async Task<List<string>> GetUserGroupsAsync(string accessToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_configuration["MicrosoftGraph:BaseUrl"]}/me/memberOf");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(content);

            var groups = new List<string>();
            foreach (var item in result.RootElement.GetProperty("value").EnumerateArray())
            {
                if (item.TryGetProperty("displayName", out var name))
                {
                    groups.Add(name.GetString());
                }
            }

            _logger.LogInformation($"User groups retrieved: {string.Join(", ", groups)}");
            return groups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user groups");
            throw;
        }
    }

    public async Task<UserEmailDTO> GetUserEmailAsync(string accessToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_configuration["MicrosoftGraph:BaseUrl"]}/me/messages?$top=1");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserEmailDTO>(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user email");
            throw;
        }
    }

    public async Task<UserPhotoDTO> GetUserPhotoAsync(string accessToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_configuration["MicrosoftGraph:BaseUrl"]}/me/photo/$value");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var photoBytes = await response.Content.ReadAsByteArrayAsync();
                return new UserPhotoDTO
                {
                    PhotoBytes = photoBytes,
                    ContentType = response.Content.Headers.ContentType?.MediaType
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user photo");
            return null;
        }
    }
}
```

---

## Authentication Controller

```csharp
[AllowAnonymous]
public class AuthenticationController : Controller
{
    private readonly IUserService _userService;
    private readonly IAzureAdService _azureAdService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IUserService userService,
        IAzureAdService azureAdService,
        ILogger<AuthenticationController> logger)
    {
        _userService = userService;
        _azureAdService = azureAdService;
        _logger = logger;
    }

    /// <summary>
    /// Login page - Azure AD'ye yönlendir
    /// </summary>
    [HttpGet("login")]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? "/"
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Logout - Çıkış yap
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation($"User {User.GetDisplayName()} logged out");

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Callback - Azure AD login sonrası
    /// </summary>
    [HttpGet("signin-oidc")]
    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Get user claims
            var userEmail = User.GetMail();
            var userName = User.GetDisplayName();
            var tenantId = User.GetTenantId();

            _logger.LogInformation($"User signed in: {userEmail}");

            // Get or create user in database
            var user = await _userService.GetOrCreateUserAsync(
                new CreateUserDTO
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FirstName = User.GetGivenName(),
                    LastName = User.GetSurname(),
                    IsOffice365User = true,
                    AzureADObjectId = User.GetObjectId()
                }
            );

            // Update last login
            user.LastLoginDate = DateTime.UtcNow;
            await _userService.UpdateAsync(user);

            // Set company from email domain or default
            var companyId = await GetCompanyIdFromEmailAsync(userEmail);
            HttpContext.Session.SetInt32("SelectedCompanyId", companyId);

            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign-in callback");
            return RedirectToAction("Error");
        }
    }

    /// <summary>
    /// Profil sayfası
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public IActionResult Profile()
    {
        var profile = new UserProfileViewModel
        {
            DisplayName = User.GetDisplayName(),
            Email = User.GetMail(),
            PhotoUrl = User.GetPhotoUrl(),
            ObjectId = User.GetObjectId()
        };

        return View(profile);
    }

    /// <summary>
    /// Multi-factor authentication gerektiğinde
    /// </summary>
    [HttpGet("mfa-required")]
    public IActionResult MfaRequired()
    {
        return View();
    }

    private async Task<int> GetCompanyIdFromEmailAsync(string email)
    {
        // Email domain'den şirket bul
        var domain = email.Split('@')[1];
        var company = await _userService.GetCompanyByDomainAsync(domain);

        return company?.CompanyID ?? 1;  // Default company
    }
}
```

---

## Claims Extension Methods

```csharp
public static class ClaimsPrincipalExtensions
{
    public static string GetObjectId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
    }

    public static string GetTenantId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
    }

    public static string GetMail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Upn)?.Value ??
               principal.FindFirst(ClaimTypes.Email)?.Value;
    }

    public static string GetDisplayName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static string GetGivenName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.GivenName)?.Value;
    }

    public static string GetSurname(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Surname)?.Value;
    }

    public static string GetPhotoUrl(this ClaimsPrincipal principal)
    {
        return principal.FindFirst("picture")?.Value;
    }

    public static List<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }
}
```

---

## User Service Updates

```csharp
public interface IUserService
{
    Task<User> GetOrCreateUserAsync(CreateUserDTO dto);
    Task<User> UpdateAsync(User user);
    Task<Company> GetCompanyByDomainAsync(string domain);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<UserService> _logger;

    public async Task<User> GetOrCreateUserAsync(CreateUserDTO dto)
    {
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user == null)
        {
            // Create new user
            user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                IsOffice365User = dto.IsOffice365User,
                AzureADObjectId = dto.AzureADObjectId,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                LastLoginDate = DateTime.UtcNow
            };

            user = await _userRepository.AddAsync(user);
            _logger.LogInformation($"New user created: {dto.Email}");
        }
        else
        {
            // Update existing user
            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.ModifiedDate = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        return user;
    }

    public async Task<Company> GetCompanyByDomainAsync(string domain)
    {
        return await _companyRepository.FirstOrDefaultAsync(c =>
            c.EmailDomain == domain && c.IsActive
        );
    }
}
```

---

## DTOs

```csharp
public class UserProfileDTO
{
    public string Id { get; set; }
    public string Mail { get; set; }
    public string DisplayName { get; set; }
    public string GivenName { get; set; }
    public string Surname { get; set; }
    public string MobilePhone { get; set; }
    public string OfficeLocation { get; set; }
}

public class UserPhotoDTO
{
    public byte[] PhotoBytes { get; set; }
    public string ContentType { get; set; }
}

public class UserEmailDTO
{
    public string Id { get; set; }
    public string Subject { get; set; }
    public string From { get; set; }
    public DateTime ReceivedDateTime { get; set; }
}

public class UserProfileViewModel
{
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string PhotoUrl { get; set; }
    public string ObjectId { get; set; }
}
```

---

## Conditional Access Policy (Optional)

Office 365 tarafında conditional access policies ayarlayabilirsiniz:

```
- Require MFA for specific users
- Require compliant devices
- Require specific locations
- Require specific app versions
```

---

## Security Best Practices

### 1. Token Management
```csharp
// Token süresini kısalt
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero  // Token süresini katı kontrol et
};
```

### 2. HTTPS Only
```csharp
// HTTPS'i zorunlu kıl
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
});
```

### 3. Secure Cookies
```json
{
  "CookiePolicy": {
    "HttpOnly": true,
    "Secure": true,
    "SameSite": "Strict"
  }
}
```

### 4. Audit Logging
```csharp
// Login attempts
_logger.LogInformation($"Login attempt: {userEmail} at {DateTime.UtcNow}");

// Failed attempts
_logger.LogWarning($"Failed login for: {userEmail}");
```

---

## Troubleshooting

### Common Issues

#### 1. "AADSTS error"
- Check Client ID, Tenant ID, Client Secret
- Verify redirect URI matches exactly

#### 2. "Sign-in requires admin approval"
- Admin consent gerekiyor
- Azure Portal → API permissions → Grant admin consent

#### 3. "Access denied"
- Check user roles
- Verify policies configured correctly

#### 4. Token expiration
- Implement refresh token logic
- Auto-renew before expiry

---

## References

- [Microsoft Identity Platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [MSAL.NET](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet)
- [Azure AD B2C](https://docs.microsoft.com/en-us/azure/active-directory-b2c/)
