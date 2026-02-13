using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        public async Task<IActionResult> Index(string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            const int pageSize = 10;

            // Tüm şirketleri al
            var allCompanies = await _companyService.GetAllAsync();
            IEnumerable<Company> query = allCompanies;

            // Arama filtresi
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    (c.CompanyName != null && c.CompanyName.ToLower().Contains(search)) ||
                    (c.CompanyCode != null && c.CompanyCode.ToLower().Contains(search)) ||
                    (c.EmailDomain != null && c.EmailDomain.ToLower().Contains(search))
                );
            }

            // Sıralama
            query = sortBy switch
            {
                "CompanyName" => sortOrder == "asc"
                    ? query.OrderBy(c => c.CompanyName)
                    : query.OrderByDescending(c => c.CompanyName),
                "CompanyCode" => sortOrder == "asc"
                    ? query.OrderBy(c => c.CompanyCode)
                    : query.OrderByDescending(c => c.CompanyCode),
                _ => query.OrderBy(c => c.CompanyName)
            };

            // Toplam sayı ve sayfa hesaplama
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Pagination
            var companies = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(companies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null) return NotFound();
            return View(company);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company)
        {
            if (ModelState.IsValid)
            {
                await _companyService.CreateAsync(company);
                AddSuccessMessage(T("company.createdSuccess"));
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null) return NotFound();
            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Company company)
        {
            if (id != company.CompanyID) return NotFound();

            if (ModelState.IsValid)
            {
                await _companyService.UpdateAsync(company);
                AddSuccessMessage(T("company.updatedSuccess"));
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null) return NotFound();
            return View(company);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _companyService.DeleteAsync(id);
            AddSuccessMessage(T("company.deletedSuccess"));
            return RedirectToAction(nameof(Index));
        }
    }
}
