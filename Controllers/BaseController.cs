using DigitalSignage.Services;
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
