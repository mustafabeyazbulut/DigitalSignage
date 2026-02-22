using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using DigitalSignage.Data;
using DigitalSignage.Services;
using DigitalSignage.Helpers;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// Language Service (JSON-based localization)
builder.Services.AddSingleton<ILanguageService, LanguageService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Memory Cache
builder.Services.AddMemoryCache();

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Multi-Tenant Services
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

// Azure AD Service (with HttpClient)
builder.Services.AddHttpClient<IAzureAdService, AzureAdService>();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en-US", "tr-TR" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// Authentication - Cookie (default) + Azure AD OpenID Connect (optional)
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
});

// Azure AD - Sadece ClientId varsa ekle
var azureAdClientId = builder.Configuration["AzureAd:ClientId"];
if (!string.IsNullOrWhiteSpace(azureAdClientId))
{
    authBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        var azureAd = builder.Configuration.GetSection("AzureAd");
        options.Authority = $"{azureAd["Instance"]}{azureAd["TenantId"]}/v2.0";
        options.ClientId = azureAdClientId;
        options.ResponseType = "id_token";
        options.CallbackPath = azureAd["CallbackPath"] ?? "/signin-oidc";
        options.SignedOutCallbackPath = "/signout-oidc";
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.TokenValidationParameters.NameClaimType = "name";
        options.Scope.Add("email");
        options.Scope.Add("profile");
    });
}

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SystemAdmin", policy =>
        policy.RequireClaim("IsSystemAdmin", "True"));

    options.AddPolicy("CompanyAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "Role" && c.Value == "CompanyAdmin")));

    options.AddPolicy("Manager", policy =>
        policy.RequireRole("Manager", "CompanyAdmin", "SystemAdmin"));
});

var app = builder.Build();

// Seed Data (SADECE Development Ortamında)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<AppDbContext>();
            context.Database.Migrate();

            // Seed Admin User
            if (!context.Users.Any())
            {
                // GÜVENLİK: Geliştirme için rastgele şifre üret
                var randomPassword = Guid.NewGuid().ToString("N").Substring(0, 12) + "Aa1!";

                context.Users.Add(new DigitalSignage.Models.User
                {
                    Email = "admin@digitalsignage.com",
                    PasswordHash = PasswordHelper.HashPassword(randomPassword),
                    FirstName = "System",
                    LastName = "Administrator",
                    IsActive = true,
                    IsSystemAdmin = true,
                    CreatedDate = DateTime.UtcNow
                });
                context.SaveChanges();

                logger.LogWarning($"DEV SEED: Admin kullanıcısı oluşturuldu. Email: admin@digitalsignage.com, Şifre: {randomPassword}");
                logger.LogWarning("UYARI: Bu şifre sadece development ortamı içindir. Production'da asla kullanmayın!");
            }

            // Seed Example Company (Development için örnek veri)
            if (!context.Companies.Any())
        {
            var company = new DigitalSignage.Models.Company
            {
                CompanyName = "Example Corporation",
                EmailDomain = "example.com",
                Description = "Sample company for demonstration",
                PrimaryColor = "#0078D4",
                SecondaryColor = "#50E6FF",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            };
            context.Companies.Add(company);
            context.SaveChanges();

            // Seed Company Configuration
            var config = new DigitalSignage.Models.CompanyConfiguration
            {
                CompanyID = company.CompanyID,
                DefaultGridColumnsX = 2,
                DefaultGridRowsY = 2,
                DefaultSectionPadding = "10px",
                MaxSchedulesPerPage = 10,
                DefaultScheduleDuration = 30,
                ScreenRefreshInterval = 5,
                EnableAutoRotation = true,
                EnableAnalytics = true,
                EnableAdvancedScheduling = true,
                EnableMediaUpload = true,
                MaxMediaSizeGB = 10,
                EmailNotificationsEnabled = false,
                NotifyOnScheduleChange = true,
                NotifyOnContentChange = true,
                NotifyOnError = true,
                CreatedDate = DateTime.UtcNow
            };
            context.CompanyConfigurations.Add(config);
            context.SaveChanges();

            // Seed Example Departments
            var departments = new[]
            {
                new DigitalSignage.Models.Department
                {
                    CompanyID = company.CompanyID,
                    DepartmentName = "Marketing",
                    Description = "Marketing department displays",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new DigitalSignage.Models.Department
                {
                    CompanyID = company.CompanyID,
                    DepartmentName = "Sales",
                    Description = "Sales department displays",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new DigitalSignage.Models.Department
                {
                    CompanyID = company.CompanyID,
                    DepartmentName = "Reception",
                    Description = "Reception area displays",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };
            context.Departments.AddRange(departments);
            context.SaveChanges();

            // Örnek Düzen Oluştur
            var layout = new DigitalSignage.Models.Layout
            {
                CompanyID = company.CompanyID,
                LayoutName = "2x2 Grid Layout",
                LayoutDefinition = "{\"rows\":[{\"height\":50,\"columns\":[{\"width\":50},{\"width\":50}]},{\"height\":50,\"columns\":[{\"width\":50},{\"width\":50}]}]}",
                Description = "Standard 2x2 grid layout",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            context.Layouts.Add(layout);
            context.SaveChanges();

            // Tanımdan düzen bölümlerini oluştur
            var sections = new List<DigitalSignage.Models.LayoutSection>
            {
                new() { LayoutID = layout.LayoutID, SectionPosition = "R1C1", ColumnIndex = 0, RowIndex = 0, Width = "50%", Height = "50%", IsActive = true },
                new() { LayoutID = layout.LayoutID, SectionPosition = "R1C2", ColumnIndex = 1, RowIndex = 0, Width = "50%", Height = "50%", IsActive = true },
                new() { LayoutID = layout.LayoutID, SectionPosition = "R2C1", ColumnIndex = 0, RowIndex = 1, Width = "50%", Height = "50%", IsActive = true },
                new() { LayoutID = layout.LayoutID, SectionPosition = "R2C2", ColumnIndex = 1, RowIndex = 1, Width = "50%", Height = "50%", IsActive = true }
            };
            context.LayoutSections.AddRange(sections);
            context.SaveChanges();

            logger.LogInformation("Örnek şirket ve veri başarıyla oluşturuldu: EXAMPLE");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Seed data hatası oluştu");
            throw;
        }
    }
}
else
{
    // Production: Sadece migration çalıştır
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:;");
    }

    await next();
});

// Session middleware (ÖNCE routing, session state'i routing'de kullanılabilir)
app.UseSession();

app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Antiforgery middleware (CSRF koruması)
app.UseAntiforgery();

// Tenant resolver middleware (authentication SONRASI)
app.UseMiddleware<DigitalSignage.Middleware.TenantResolverMiddleware>();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
