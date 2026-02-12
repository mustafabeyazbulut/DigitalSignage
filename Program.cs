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

// Authentication - Cookie (default) + Azure AD OpenID Connect
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    var azureAd = builder.Configuration.GetSection("AzureAd");
    options.Authority = $"{azureAd["Instance"]}{azureAd["TenantId"]}/v2.0";
    options.ClientId = azureAd["ClientId"];
    options.ResponseType = "id_token";
    options.CallbackPath = azureAd["CallbackPath"] ?? "/signin-oidc";
    options.SignedOutCallbackPath = "/signout-oidc";
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.TokenValidationParameters.NameClaimType = "name";
    options.Scope.Add("email");
    options.Scope.Add("profile");
});

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

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        // Seed Admin User
        if (!context.Users.Any())
        {
            context.Users.Add(new DigitalSignage.Models.User
            {
                UserName = "admin",
                Email = "admin@digitalsignage.com",
                PasswordHash = PasswordHelper.HashPassword("admin123"),
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true,
                IsSystemAdmin = true,
                CreatedDate = DateTime.UtcNow
            });
            context.SaveChanges();
        }

        // Seed Example Company
        if (!context.Companies.Any())
        {
            var company = new DigitalSignage.Models.Company
            {
                CompanyName = "Example Corporation",
                CompanyCode = "EXAMPLE",
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
                    DepartmentCode = "MKT",
                    Description = "Marketing department displays",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new DigitalSignage.Models.Department
                {
                    CompanyID = company.CompanyID,
                    DepartmentName = "Sales",
                    DepartmentCode = "SALES",
                    Description = "Sales department displays",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new DigitalSignage.Models.Department
                {
                    CompanyID = company.CompanyID,
                    DepartmentName = "Reception",
                    DepartmentCode = "RCP",
                    Description = "Reception area displays",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            };
            context.Departments.AddRange(departments);
            context.SaveChanges();

            // Seed Example Layout
            var layout = new DigitalSignage.Models.Layout
            {
                CompanyID = company.CompanyID,
                LayoutName = "2x2 Grid Layout",
                LayoutCode = "GRID_2X2",
                GridColumnsX = 2,
                GridRowsY = 2,
                Description = "Standard 2x2 grid layout",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            context.Layouts.Add(layout);
            context.SaveChanges();

            // Seed Layout Sections
            var sections = new List<DigitalSignage.Models.LayoutSection>();
            for (int row = 0; row < layout.GridRowsY; row++)
            {
                for (int col = 0; col < layout.GridColumnsX; col++)
                {
                    sections.Add(new DigitalSignage.Models.LayoutSection
                    {
                        LayoutID = layout.LayoutID,
                        SectionPosition = $"{(char)('A' + row)}{col + 1}",
                        ColumnIndex = col,
                        RowIndex = row,
                        Width = "100%",
                        Height = "100%",
                        IsActive = true
                    });
                }
            }
            context.LayoutSections.AddRange(sections);
            context.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseRouting();

// Session middleware (must be before authentication)
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Tenant resolver middleware (after authentication)
app.UseMiddleware<DigitalSignage.Middleware.TenantResolverMiddleware>();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
