using DigitalSignage.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string userName, string password, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                AddErrorMessage("Username and password are required.");
                return View();
            }

            var isValid = await _userService.AuthenticateAsync(userName, password);
            if (isValid)
            {
                var user = await _userService.GetByUserNameAsync(userName);
                
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("UserId", user.UserID.ToString())
                    // Rol bilgileri burada eklenebilir
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Beni hatırla özelliği için true yapılabilir
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(60)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                AddSuccessMessage($"Welcome back, {user.UserName}!");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            AddErrorMessage("Invalid username or password.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
