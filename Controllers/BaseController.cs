using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using Microsoft.AspNetCore.Authorization;

namespace DigitalSignage.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        // Burada ileride Tenant Context veya User Context işlemleri yapılabilir.
        // Örneğin her request'te CompanyID'yi almak gibi.

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            // Global işlemler buraya
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
