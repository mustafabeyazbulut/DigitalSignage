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
