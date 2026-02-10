using Microsoft.EntityFrameworkCore;
using DigitalSignage.Data;
using DigitalSignage.Data.Repositories;
using DigitalSignage.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<DigitalSignage.Data.AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped(typeof(DigitalSignage.Data.Repositories.IRepository<>), typeof(DigitalSignage.Data.Repositories.Repository<>));
builder.Services.AddScoped<DigitalSignage.Data.Repositories.IUserRepository, DigitalSignage.Data.Repositories.UserRepository>();
builder.Services.AddScoped<DigitalSignage.Data.Repositories.ICompanyRepository, DigitalSignage.Data.Repositories.CompanyRepository>();
builder.Services.AddScoped<DigitalSignage.Data.Repositories.IDepartmentRepository, DigitalSignage.Data.Repositories.DepartmentRepository>();
builder.Services.AddScoped<DigitalSignage.Data.Repositories.IPageRepository, DigitalSignage.Data.Repositories.PageRepository>();
builder.Services.AddScoped<DigitalSignage.Data.Repositories.ILayoutRepository, DigitalSignage.Data.Repositories.LayoutRepository>();
builder.Services.AddScoped<DigitalSignage.Data.Repositories.IContentRepository, DigitalSignage.Data.Repositories.ContentRepository>();
builder.Services.AddScoped<DigitalSignage.Data.Repositories.IScheduleRepository, DigitalSignage.Data.Repositories.ScheduleRepository>();

// Add Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en-US", "tr-TR" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

// Add Services
builder.Services.AddScoped<DigitalSignage.Services.IUserService, DigitalSignage.Services.UserService>();
builder.Services.AddScoped<DigitalSignage.Services.ICompanyService, DigitalSignage.Services.CompanyService>();
builder.Services.AddScoped<DigitalSignage.Services.IDepartmentService, DigitalSignage.Services.DepartmentService>();
builder.Services.AddScoped<DigitalSignage.Services.IPageService, DigitalSignage.Services.PageService>();
builder.Services.AddScoped<DigitalSignage.Services.ILayoutService, DigitalSignage.Services.LayoutService>();
builder.Services.AddScoped<DigitalSignage.Services.IContentService, DigitalSignage.Services.ContentService>();
builder.Services.AddScoped<DigitalSignage.Services.IScheduleService, DigitalSignage.Services.ScheduleService>();

// Add Authentication
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// ... (Existing Service Registrations)

var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DigitalSignage.Data.AppDbContext>();
        context.Database.Migrate(); // Ensure database is created/migrated

        if (!context.Users.Any())
        {
            context.Users.Add(new DigitalSignage.Models.User
            {
                UserName = "admin",
                Email = "admin@digitalsignage.com",
                PasswordHash = "admin123", // DEV ONLY: Plain text for now
                IsActive = true,
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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
