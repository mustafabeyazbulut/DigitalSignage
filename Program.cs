using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using DigitalSignage.Data;
using DigitalSignage.Services;
using DigitalSignage.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

// Language Service (JSON-based localization)
builder.Services.AddSingleton<ILanguageService, LanguageService>();

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

var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();

        if (!context.Users.Any())
        {
            context.Users.Add(new DigitalSignage.Models.User
            {
                UserName = "admin",
                Email = "admin@digitalsignage.com",
                PasswordHash = PasswordHelper.HashPassword("admin123"),
                IsActive = true,
                IsSystemAdmin = true,
                CreatedDate = DateTime.UtcNow
            });
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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
