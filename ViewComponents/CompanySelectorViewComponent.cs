using System.Security.Claims;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.ViewComponents
{
    public class CompanySelectorViewComponent : ViewComponent
    {
        private readonly IAuthorizationService _authService;

        public CompanySelectorViewComponent(IAuthorizationService authService)
        {
            _authService = authService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = GetCurrentUserId();
            var activeCompanies = userId > 0
                ? await _authService.GetUserCompaniesAsync(userId)
                : new List<Models.Company>();

            // Get selected company from session
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");

            // Seçili şirket kullanıcının erişebildikleri arasında değilse sıfırla
            if (selectedCompanyId.HasValue && !activeCompanies.Any(c => c.CompanyID == selectedCompanyId.Value))
            {
                selectedCompanyId = activeCompanies.FirstOrDefault()?.CompanyID;
                if (selectedCompanyId.HasValue)
                    HttpContext.Session.SetInt32("SelectedCompanyId", selectedCompanyId.Value);
                else
                    HttpContext.Session.Remove("SelectedCompanyId");
            }

            // If no company selected, use first accessible company
            if (!selectedCompanyId.HasValue && activeCompanies.Any())
            {
                selectedCompanyId = activeCompanies.First().CompanyID;
                HttpContext.Session.SetInt32("SelectedCompanyId", selectedCompanyId.Value);
            }

            var selectedCompany = activeCompanies.FirstOrDefault(c => c.CompanyID == selectedCompanyId);

            ViewBag.Companies = activeCompanies;
            ViewBag.SelectedCompany = selectedCompany;
            ViewBag.SelectedCompanyId = selectedCompanyId;

            return View(selectedCompany);
        }

        private int GetCurrentUserId()
        {
            var claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }
}
