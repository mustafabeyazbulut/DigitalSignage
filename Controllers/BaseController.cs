using DigitalSignage.Services;
using DigitalSignage.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using AuthService = DigitalSignage.Services.IAuthorizationService;

namespace DigitalSignage.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        /// <summary>
        /// Dil servisi - tüm controller'lar tarafından kullanılır.
        /// </summary>
        protected ILanguageService? _languageService;

        /// <summary>
        /// Aktif dil kodu (cookie'den veya varsayılan)
        /// </summary>
        protected string CurrentLocale => HttpContext?.Request.Cookies["locale"] ?? "en";

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // DI'dan LanguageService'i al
            _languageService = HttpContext.RequestServices.GetService<ILanguageService>();

            // View'lara dil bilgilerini aktar
            ViewBag.CurrentLocale = CurrentLocale;
            ViewBag.Lang = _languageService;
            ViewBag.SupportedLanguages = _languageService?.GetSupportedLanguages();

            // Smart back navigation support
            SetupSmartBackNavigation(context);

            // Breadcrumb navigation support
            SetupBreadcrumbNavigation(context);

            // Rol bazlı ViewBag değerlerini ayarla (navigasyon ve buton görünürlüğü için)
            await SetupRoleFlags();

            // POST isteklerinde ModelState hatalarını aktif dile çevir
            if (HttpContext.Request.Method == "POST")
            {
                LocalizeModelStateErrors();
            }

            await next();
        }

        private async Task SetupRoleFlags()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                ViewBag.IsSystemAdmin = false;
                ViewBag.IsCompanyAdmin = false;
                ViewBag.HasAnyRole = false;
                return;
            }

            var isSystemAdmin = User.FindFirst("IsSystemAdmin")?.Value == "True";
            ViewBag.IsSystemAdmin = isSystemAdmin;

            if (isSystemAdmin)
            {
                ViewBag.IsCompanyAdmin = true;
                ViewBag.HasAnyRole = true;
                return;
            }

            var authService = HttpContext.RequestServices.GetService<AuthService>();
            if (authService != null)
            {
                var userId = GetCurrentUserId();
                ViewBag.IsCompanyAdmin = await authService.HasAnyCompanyAdminRoleAsync(userId);
                ViewBag.HasAnyRole = await authService.HasAnyRoleAsync(userId);
            }
            else
            {
                ViewBag.IsCompanyAdmin = false;
                ViewBag.HasAnyRole = false;
            }
        }

        /// <summary>
        /// Smart back navigation için returnUrl mantığını ayarlar.
        /// Her sayfada akıllı geri dönüş desteği sağlar.
        /// </summary>
        private void SetupSmartBackNavigation(ActionExecutingContext context)
        {
            var request = HttpContext.Request;
            var currentUrl = $"{request.Path}{request.QueryString}";

            // Get referrer (previous page)
            var referrer = request.Headers["Referer"].ToString();

            // Default fallback URL based on controller
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            string defaultFallback = "/";

            // Set intelligent fallback based on current location
            if (!string.IsNullOrEmpty(controllerName))
            {
                // For Create/Edit/Details pages, fallback to Index
                if (actionName == "Create" || actionName == "Edit" || actionName == "Details")
                {
                    defaultFallback = $"/{controllerName}/Index";
                }
                // For Profile/Settings/AccessDenied/Login, fallback to Home
                else if (controllerName == "Account" &&
                        (actionName == "Profile" || actionName == "Settings" || actionName == "ChangePassword" ||
                         actionName == "AccessDenied" || actionName == "Login"))
                {
                    defaultFallback = "/Home/Index";
                }
                // For other pages, fallback to controller index
                else if (actionName != "Index")
                {
                    defaultFallback = $"/{controllerName}/Index";
                }
            }

            // Set ViewBag values for views to use
            ViewBag.ReturnUrl = referrer;
            ViewBag.DefaultReturnUrl = defaultFallback;
            ViewBag.CurrentUrl = currentUrl;
        }

        /// <summary>
        /// Breadcrumb navigation için otomatik breadcrumb item'ları oluşturur.
        /// Controller ve Action bazlı dinamik breadcrumb desteği sağlar.
        /// </summary>
        private void SetupBreadcrumbNavigation(ActionExecutingContext context)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();
            var id = context.RouteData.Values["id"]?.ToString();

            var breadcrumbs = new List<BreadcrumbItem>
            {
                // Always start with Home
                new BreadcrumbItem
                {
                    Text = T("nav.dashboard"),
                    Url = "/Home/Index",
                    Icon = "fas fa-home",
                    IsActive = false
                }
            };

            // Special handling for Account controller (Profile, Settings, ChangePassword, AccessDenied, Login)
            // These don't have Index action, so breadcrumb should be: Home > Profile/Settings/AccessDenied
            bool isAccountSpecialAction = controllerName == "Account" &&
                (actionName == "Profile" || actionName == "Settings" || actionName == "ChangePassword" ||
                 actionName == "AccessDenied" || actionName == "Login");

            // Add controller-level breadcrumb
            if (!string.IsNullOrEmpty(controllerName) && controllerName != "Home" && !isAccountSpecialAction)
            {
                string controllerText = GetControllerDisplayName(controllerName);
                string controllerUrl = $"/{controllerName}/Index";

                breadcrumbs.Add(new BreadcrumbItem
                {
                    Text = controllerText,
                    Url = actionName == "Index" ? null : controllerUrl,
                    Icon = GetControllerIcon(controllerName),
                    IsActive = actionName == "Index"
                });
            }

            // Add action-level breadcrumb
            if (!string.IsNullOrEmpty(actionName) && actionName != "Index" && controllerName != "Home")
            {
                string actionText = GetActionDisplayName(actionName);
                string actionIcon = GetActionIcon(actionName);

                // For Account special actions, add directly (no controller breadcrumb)
                if (isAccountSpecialAction)
                {
                    breadcrumbs.Add(new BreadcrumbItem
                    {
                        Text = actionText,
                        Url = null,
                        Icon = actionIcon,
                        IsActive = true
                    });
                }
                else
                {
                    breadcrumbs.Add(new BreadcrumbItem
                    {
                        Text = actionText,
                        Url = null,
                        Icon = actionIcon,
                        IsActive = true
                    });
                }
            }

            // If only Home, mark it as active
            if (controllerName == "Home" && actionName == "Index")
            {
                breadcrumbs[0].IsActive = true;
            }

            ViewBag.BreadcrumbItems = breadcrumbs;

            // Set breadcrumb-aware back URL (parent breadcrumb item)
            // Öncelikle returnUrl query parametresini kontrol et (deep linking)
            var request = context.HttpContext.Request;
            var returnUrl = request.Query["returnUrl"].ToString();

            string breadcrumbBackUrl = "/";

            // returnUrl varsa onu kullan (en yüksek öncelik)
            if (!string.IsNullOrEmpty(returnUrl))
            {
                breadcrumbBackUrl = returnUrl;
            }
            // Yoksa breadcrumb hiyerarşisinden hesapla
            else if (breadcrumbs.Count > 1)
            {
                // Son aktif olmayan breadcrumb'ı bul (parent)
                var parentBreadcrumb = breadcrumbs[breadcrumbs.Count - 2];
                breadcrumbBackUrl = parentBreadcrumb.Url ?? "/";
            }

            ViewBag.BreadcrumbBackUrl = breadcrumbBackUrl;
        }

        /// <summary>
        /// Controller adını localized display name'e çevirir.
        /// </summary>
        private string GetControllerDisplayName(string controllerName)
        {
            return controllerName?.ToLower() switch
            {
                "company" => T("nav.companies"),
                "department" => T("nav.departments"),
                "user" => T("nav.users"),
                "page" => T("nav.pages"),
                "layout" => T("nav.layouts"),
                "content" => T("nav.mediaLibrary"),
                "schedule" => T("nav.schedules"),
                "account" => T("nav.profile"),
                _ => controllerName ?? "Unknown"
            };
        }

        /// <summary>
        /// Action adını localized display name'e çevirir.
        /// </summary>
        private string GetActionDisplayName(string actionName)
        {
            return actionName?.ToLower() switch
            {
                "create" => T("common.create"),
                "edit" => T("common.edit"),
                "details" => T("common.details"),
                "delete" => T("common.delete"),
                "profile" => T("nav.profile"),
                "settings" => T("nav.settings"),
                "changepassword" => T("settings.changePassword"),
                "manageroles" => T("user.manageRoles"),
                _ => actionName ?? "Unknown"
            };
        }

        /// <summary>
        /// Controller için uygun icon döner.
        /// </summary>
        private string GetControllerIcon(string controllerName)
        {
            return controllerName?.ToLower() switch
            {
                "company" => "fas fa-building",
                "department" => "fas fa-sitemap",
                "user" => "fas fa-users",
                "page" => "fas fa-desktop",
                "layout" => "fas fa-th-large",
                "content" => "fas fa-photo-video",
                "schedule" => "fas fa-calendar-alt",
                "account" => "fas fa-user-circle",
                _ => "fas fa-folder"
            };
        }

        /// <summary>
        /// Action için uygun icon döner.
        /// </summary>
        private string GetActionIcon(string actionName)
        {
            return actionName?.ToLower() switch
            {
                "create" => "fas fa-plus",
                "edit" => "fas fa-edit",
                "details" => "fas fa-info-circle",
                "delete" => "fas fa-trash",
                "profile" => "fas fa-user-circle",
                "settings" => "fas fa-cog",
                "changepassword" => "fas fa-key",
                "manageroles" => "fas fa-user-shield",
                _ => "fas fa-file"
            };
        }

        /// <summary>
        /// ModelState alan adlarını kullanıcı dostu, lokalize edilmiş isimlere çevirir.
        /// Eşleşme bulunamazsa alan adını PascalCase'den ayırarak döner (ör. "CompanyName" → "Company Name").
        /// </summary>
        private string GetFieldDisplayName(string fieldName)
        {
            // Alan adı → çeviri anahtarı eşleştirmesi
            var displayName = fieldName switch
            {
                // Ortak alanlar
                "CompanyID" or "CompanyId" => T("common.company"),
                "DepartmentID" or "DepartmentId" => T("common.department"),
                "IsActive" => T("common.status"),
                "Description" => T("company.description"),

                // Şirket alanları
                "CompanyName" => T("company.companyName"),
                "CompanyCode" => T("company.companyCode"),
                "EmailDomain" => T("company.emailDomain"),
                "LogoUrl" => T("company.logoUrl"),
                "PrimaryColor" => T("company.primaryColor"),
                "SecondaryColor" => T("company.secondaryColor"),

                // Departman alanları
                "DepartmentName" => T("department.departmentName"),
                "DepartmentCode" => T("department.code"),

                // Düzen alanları
                "LayoutID" or "LayoutId" => T("page.layout"),
                "LayoutName" => T("layout.layoutName"),
                "LayoutDefinition" => T("layout.defineGrid"),

                // Sayfa alanları
                "PageName" => T("page.pageName"),
                "PageTitle" => T("page.pageTitle"),
                "PageCode" => T("page.pageCode"),

                // İçerik alanları
                "ContentType" => T("content.contentType"),
                "ContentTitle" => T("content.contentTitle"),
                "ContentData" => T("content.contentData"),
                "MediaPath" => T("content.mediaPath"),

                // Zamanlama alanları
                "ScheduleName" => T("schedule.scheduleName"),
                "StartDate" => T("schedule.startDate"),
                "EndDate" => T("schedule.endDate"),
                "StartTime" => T("schedule.startTime"),
                "EndTime" => T("schedule.endTime"),
                "RecurrencePattern" => T("schedule.recurrencePattern"),

                // Kullanıcı alanları
                "Email" => T("user.email"),
                "Password" => T("user.password"),
                "FirstName" => T("user.firstName"),
                "LastName" => T("user.lastName"),

                // Eşleşme yoksa: PascalCase'i boşluklarla ayır (ör. "ContentTitle" → "Content Title")
                _ => System.Text.RegularExpressions.Regex.Replace(fieldName, "([a-z])([A-Z])", "$1 $2")
            };

            return displayName;
        }

        /// <summary>
        /// ModelState'teki İngilizce varsayılan hata mesajlarını aktif dile çevirir.
        /// Alan adı bazlı mesajlar üretir (ör. "Departman" alanı zorunludur.).
        /// POST isteklerinde otomatik çalışır (OnActionExecutionAsync'den çağrılır).
        /// </summary>
        private void LocalizeModelStateErrors()
        {
            foreach (var entry in ModelState)
            {
                var errors = entry.Value?.Errors.ToList();
                if (errors == null || errors.Count == 0) continue;

                // Alan adını çeviri anahtarından çöz
                var fieldName = entry.Key;
                // Nested property ise son kısmı al (ör. "Model.CompanyID" → "CompanyID")
                if (fieldName.Contains('.'))
                    fieldName = fieldName.Split('.').Last();

                var displayName = GetFieldDisplayName(fieldName);

                entry.Value!.Errors.Clear();
                foreach (var error in errors)
                {
                    var msg = error.ErrorMessage;

                    // "The value '' is invalid." veya "The value 'x' is not valid for Y."
                    if (msg.StartsWith("The value") && (msg.Contains("is not valid") || msg.Contains("is invalid")))
                    {
                        var localizedMsg = string.Format(T("validation.fieldInvalidValue"), displayName);
                        entry.Value.Errors.Add(new ModelError(localizedMsg));
                    }
                    // "The X field is required."
                    else if (msg.Contains("field is required") || msg.Contains("is required"))
                    {
                        var localizedMsg = string.Format(T("validation.fieldRequired"), displayName);
                        entry.Value.Errors.Add(new ModelError(localizedMsg));
                    }
                    // "The field X must be between Y and Z." (Range)
                    else if (msg.Contains("must be between"))
                    {
                        var localizedMsg = string.Format(T("validation.fieldRange"), displayName);
                        entry.Value.Errors.Add(new ModelError(localizedMsg));
                    }
                    // "The field X must be a string with a maximum length of Y." (MaxLength)
                    else if (msg.Contains("maximum length"))
                    {
                        var localizedMsg = string.Format(T("validation.fieldMaxLength"), displayName);
                        entry.Value.Errors.Add(new ModelError(localizedMsg));
                    }
                    else
                    {
                        // Tanınmayan mesajları olduğu gibi bırak
                        entry.Value.Errors.Add(error);
                    }
                }
            }
        }

        /// <summary>
        /// View'larda kullanılmak üzere çeviri kısayolu.
        /// Controller'dan TempData mesajlarında kullanılabilir.
        /// </summary>
        protected string T(string key)
        {
            return _languageService?.Get(CurrentLocale, key) ?? key;
        }

        protected void AddSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void AddErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        /// <summary>
        /// Aktif kullanıcının ID'sini claim'den okur.
        /// </summary>
        protected int GetCurrentUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        /// <summary>
        /// Erişim reddedildi sayfasına yönlendirir.
        /// </summary>
        protected IActionResult AccessDenied()
        {
            return RedirectToAction("AccessDenied", "Account");
        }
    }
}
