using DigitalSignage.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DigitalSignage.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                AddErrorMessage(T("auth.requiredFields"));
                return View();
            }

            var user = await _userService.AuthenticateAsync(email, password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserId", user.UserID.ToString()),
                    new Claim("IsSystemAdmin", user.IsSystemAdmin.ToString()),
                    new Claim("AuthMethod", "Local")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                AddSuccessMessage(string.Format(T("auth.welcomeBack"), user.UserName));

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            AddErrorMessage(T("auth.invalidCredentials"));
            return View();
        }

        /// <summary>
        /// Office 365 / Azure AD SSO ile giriş başlatır.
        /// OpenID Connect challenge'ı tetikler → Microsoft login sayfasına yönlendirir.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                Items = { { "returnUrl", returnUrl ?? "/" } }
            };
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Microsoft login sonrası callback. Azure AD'den dönen bilgileri işler,
        /// kullanıcıyı cookie ile oturum açtırır.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
        {
            // Azure AD auth sonrası HttpContext.User zaten dolu gelir
            var result = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                AddErrorMessage(T("auth.ssoFailed"));
                return RedirectToAction("Login");
            }

            var externalClaims = result.Principal?.Claims.ToList() ?? new List<Claim>();

            // Azure AD'den gelen bilgileri al
            var email = externalClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email
                || c.Type == "preferred_username")?.Value ?? "";
            var name = externalClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name
                || c.Type == "name")?.Value ?? email;
            var objectId = externalClaims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? "";

            // Veritabanında kayıtlı kullanıcı var mı kontrol et (ZORUNLU)
            var existingUser = await _userService.GetByEmailAsync(email);
            if (existingUser == null)
            {
                // SSO kullanıcısı veritabanında kayıtlı değilse erişimi reddet
                AddErrorMessage(T("auth.ssoNotRegistered"));
                return RedirectToAction("Login");
            }

            if (!existingUser.IsActive)
            {
                AddErrorMessage(T("auth.accountDisabled"));
                return RedirectToAction("Login");
            }

            // Cookie claim'leri oluştur (veritabanındaki bilgilerle)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, existingUser.UserID.ToString()),
                new Claim(ClaimTypes.Name, existingUser.UserName),
                new Claim(ClaimTypes.Email, existingUser.Email),
                new Claim("UserId", existingUser.UserID.ToString()),
                new Claim("IsSystemAdmin", existingUser.IsSystemAdmin.ToString()),
                new Claim("AzureAdObjectId", objectId),
                new Claim("AuthMethod", "AzureAD")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(8)
            };

            // OpenID Connect oturumunu temizle, Cookie oturumunu aç
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            AddSuccessMessage(string.Format(T("auth.welcomeBack"), existingUser.UserName));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user != null)
                {
                    return View(user);
                }
            }

            AddErrorMessage(T("user.notFound"));
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Settings()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user != null)
                {
                    return View(user);
                }
            }

            AddErrorMessage(T("user.notFound"));
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateSettings(bool emailNotificationsEnabled = false)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    var user = await _userService.GetByIdAsync(userId);
                    if (user != null)
                    {
                        user.EmailNotificationsEnabled = emailNotificationsEnabled;
                        user.ModifiedDate = DateTime.UtcNow;
                        await _userService.UpdateAsync(user);

                        AddSuccessMessage(T("settings.settingsSaved"));
                        return RedirectToAction(nameof(Settings));
                    }
                }

                AddErrorMessage(T("user.notFound"));
                return RedirectToAction(nameof(Settings));
            }
            catch (Exception)
            {
                AddErrorMessage(T("settings.errorSaving"));
                return RedirectToAction(nameof(Settings));
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangePassword()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user != null)
                {
                    // Office 365 users cannot change password here
                    if (user.IsOffice365User)
                    {
                        AddErrorMessage(T("settings.office365PasswordNote"));
                        return RedirectToAction(nameof(Settings));
                    }
                    return View(user);
                }
            }

            AddErrorMessage(T("user.notFound"));
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                AddErrorMessage(T("user.notFound"));
                return RedirectToAction("Index", "Home");
            }

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
            {
                AddErrorMessage(T("user.notFound"));
                return RedirectToAction("Index", "Home");
            }

            // Office 365 users cannot change password
            if (user.IsOffice365User)
            {
                AddErrorMessage(T("settings.office365PasswordNote"));
                return RedirectToAction(nameof(Settings));
            }

            // Validate inputs
            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                AddErrorMessage(T("auth.requiredFields"));
                return View(user);
            }

            // Check password length
            if (newPassword.Length < 6)
            {
                AddErrorMessage(T("settings.passwordRequirements"));
                return View(user);
            }

            // Check if new passwords match
            if (newPassword != confirmPassword)
            {
                AddErrorMessage(T("settings.passwordMismatch"));
                return View(user);
            }

            // Attempt to change password
            var success = await _userService.ChangePasswordAsync(userId, currentPassword, newPassword);
            if (success)
            {
                AddSuccessMessage(T("settings.passwordChanged"));
                return RedirectToAction(nameof(Settings));
            }

            AddErrorMessage(T("settings.incorrectPassword"));
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            AddSuccessMessage(T("auth.loggedOut"));
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// Switches the current company context globally.
        /// Updates session and redirects back to the referring page.
        /// </summary>
        [HttpGet]
        public IActionResult SwitchCompany(int companyId, string? returnUrl = null)
        {
            // Update the selected company in session
            HttpContext.Session.SetInt32("SelectedCompanyId", companyId);

            // Redirect to the return URL or home
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
