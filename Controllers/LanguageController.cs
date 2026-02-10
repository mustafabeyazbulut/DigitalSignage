using DigitalSignage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Controllers
{
    /// <summary>
    /// Dil yönetimi controller'ı.
    /// Dil değiştirme ve çeviri JSON erişimi sağlar.
    /// </summary>
    public class LanguageController : Controller
    {
        private readonly ILanguageService _languageService;

        public LanguageController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        /// <summary>
        /// Aktif dili değiştirir (cookie ile kaydeder) ve önceki sayfaya yönlendirir.
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Change(string locale, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(locale))
                locale = "en";

            // Desteklenen diller arasında mı kontrol et
            var supported = _languageService.GetSupportedLanguages();
            if (!supported.Contains(locale))
                locale = "en";

            // Cookie ile dil tercihini kaydet (1 yıl geçerli)
            Response.Cookies.Append("locale", locale, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = false, // JS tarafından da okunabilir
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

            // Yönlendirme
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Belirtilen dilin tüm çevirilerini JSON olarak döner (AJAX kullanımı için).
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Json(string locale)
        {
            if (string.IsNullOrEmpty(locale))
                locale = "en";

            var json = _languageService.GetAllAsJson(locale);
            return Content(json, "application/json");
        }
    }
}
