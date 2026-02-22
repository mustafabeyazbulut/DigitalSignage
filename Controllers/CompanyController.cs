using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;
using AuthService = DigitalSignage.Services.IAuthorizationService;

namespace DigitalSignage.Controllers
{
    public class CompanyController : BaseController
    {
        private readonly ICompanyService _companyService;
        private readonly AuthService _authService;
        private readonly IFileStorageService _fileStorage;

        public CompanyController(ICompanyService companyService, AuthService authService, IFileStorageService fileStorage)
        {
            _companyService = companyService;
            _authService = authService;
            _fileStorage = fileStorage;
        }

        public async Task<IActionResult> Index(string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            var userId = GetCurrentUserId();
            var isSystemAdmin = await _authService.IsSystemAdminAsync(userId);

            // Sadece SystemAdmin şirket listesini görebilir
            if (!isSystemAdmin)
                return AccessDenied();

            const int pageSize = 10;

            var allCompanies = await _companyService.GetAllAsync();
            IEnumerable<Company> query = allCompanies;

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    (c.CompanyName != null && c.CompanyName.ToLower().Contains(search)) ||
                    (c.EmailDomain != null && c.EmailDomain.ToLower().Contains(search))
                );
            }

            query = sortBy switch
            {
                "CompanyName" => sortOrder == "asc"
                    ? query.OrderBy(c => c.CompanyName)
                    : query.OrderByDescending(c => c.CompanyName),
                _ => query.OrderBy(c => c.CompanyName)
            };

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var companies = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(companies);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();

            // SystemAdmin veya o şirketin CompanyAdmin'i görebilir
            if (!await _authService.IsCompanyAdminAsync(userId, id))
                return AccessDenied();

            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
            {
                AddErrorMessage(T("company.notFound"));
                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        public async Task<IActionResult> Create()
        {
            var userId = GetCurrentUserId();

            if (!await _authService.IsSystemAdminAsync(userId))
                return AccessDenied();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company, IFormFile? logoFile)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.IsSystemAdminAsync(userId))
                return AccessDenied();

            // Form'da gelmeyen non-nullable property'leri ModelState'ten çıkar
            ModelState.Remove("Departments");
            ModelState.Remove("Layouts");
            ModelState.Remove("UserCompanyRoles");
            ModelState.Remove("CreatedBy");

            // Logo dosyası varsa doğrula
            if (logoFile != null && !_fileStorage.IsValidImageFile(logoFile))
            {
                ModelState.AddModelError("logoFile", T("company.invalidLogoFile"));
            }

            if (ModelState.IsValid)
            {
                // Önce şirketi kaydet (CompanyID oluşsun)
                await _companyService.CreateAsync(company);

                // Logo dosyası varsa yükle ve LogoPath'i güncelle
                if (logoFile != null)
                {
                    company.LogoPath = await _fileStorage.SaveFileAsync(logoFile, company.CompanyID);
                    await _companyService.UpdateAsync(company);
                }

                AddSuccessMessage(T("company.createdSuccess"));
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.IsSystemAdminAsync(userId))
                return AccessDenied();

            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
            {
                AddErrorMessage(T("company.notFound"));
                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Company company, IFormFile? logoFile)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.IsSystemAdminAsync(userId))
                return AccessDenied();

            if (id != company.CompanyID)
            {
                AddErrorMessage(T("company.notFound"));
                return RedirectToAction(nameof(Index));
            }

            // Form'da gelmeyen non-nullable property'leri ModelState'ten çıkar
            ModelState.Remove("Departments");
            ModelState.Remove("Layouts");
            ModelState.Remove("UserCompanyRoles");
            ModelState.Remove("CreatedBy");

            // Logo dosyası varsa doğrula
            if (logoFile != null && !_fileStorage.IsValidImageFile(logoFile))
            {
                ModelState.AddModelError("logoFile", T("company.invalidLogoFile"));
            }

            if (ModelState.IsValid)
            {
                // Yeni logo dosyası yüklendiyse eskiyi sil, yenisini kaydet
                if (logoFile != null)
                {
                    _fileStorage.DeleteFile(company.LogoPath);
                    company.LogoPath = await _fileStorage.SaveFileAsync(logoFile, company.CompanyID);
                }

                await _companyService.UpdateAsync(company);
                AddSuccessMessage(T("company.updatedSuccess"));
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.IsSystemAdminAsync(userId))
                return AccessDenied();

            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
            {
                AddErrorMessage(T("company.notFound"));
                return RedirectToAction(nameof(Index));
            }

            return View(company);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.IsSystemAdminAsync(userId))
                return AccessDenied();

            // Silmeden önce logo dosyasını temizle
            var company = await _companyService.GetByIdAsync(id);
            if (company != null)
            {
                _fileStorage.DeleteFile(company.LogoPath);
            }

            await _companyService.DeleteAsync(id);
            AddSuccessMessage(T("company.deletedSuccess"));
            return RedirectToAction(nameof(Index));
        }
    }
}
