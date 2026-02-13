using DigitalSignage.Services;
using DigitalSignage.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;

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

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

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
                // For Profile/Settings, fallback to Home
                else if (controllerName == "Account" && (actionName == "Profile" || actionName == "Settings" || actionName == "ChangePassword"))
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

            // Add controller-level breadcrumb
            if (!string.IsNullOrEmpty(controllerName) && controllerName != "Home")
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

                breadcrumbs.Add(new BreadcrumbItem
                {
                    Text = actionText,
                    Url = null,
                    Icon = GetActionIcon(actionName),
                    IsActive = true
                });
            }

            // If only Home, mark it as active
            if (controllerName == "Home" && actionName == "Index")
            {
                breadcrumbs[0].IsActive = true;
            }

            ViewBag.BreadcrumbItems = breadcrumbs;
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
                _ => "fas fa-file"
            };
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
    }
}
