using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.ViewComponents
{
    public class CompanySelectorViewComponent : ViewComponent
    {
        private readonly ICompanyService _companyService;

        public CompanySelectorViewComponent(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var companies = await _companyService.GetAllAsync();
            var activeCompanies = companies.Where(c => c.IsActive).ToList();

            // Get selected company from session
            var selectedCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");

            // If no company selected, use first active company
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
    }
}
