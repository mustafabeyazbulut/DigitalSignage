using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Controllers
{
    public class DepartmentController : BaseController
    {
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;

        public DepartmentController(IDepartmentService departmentService, ICompanyService companyService)
        {
            _departmentService = departmentService;
            _companyService = companyService;
        }

        public async Task<IActionResult> Index(int? companyId, string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            const int pageSize = 10;

            // Şirkete göre veya tüm departmanları al
            IEnumerable<Department> allDepartments;
            if (companyId.HasValue)
            {
                allDepartments = await _departmentService.GetByCompanyIdAsync(companyId.Value);
                ViewBag.CompanyId = companyId;
            }
            else
            {
                allDepartments = await _departmentService.GetAllAsync();
            }

            IEnumerable<Department> query = allDepartments;

            // Arama filtresi
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(d =>
                    (d.DepartmentName != null && d.DepartmentName.ToLower().Contains(search)) ||
                    (d.DepartmentCode != null && d.DepartmentCode.ToLower().Contains(search)) ||
                    (d.Company?.CompanyName != null && d.Company.CompanyName.ToLower().Contains(search))
                );
            }

            // Sıralama
            query = sortBy switch
            {
                "DepartmentName" => sortOrder == "asc"
                    ? query.OrderBy(d => d.DepartmentName)
                    : query.OrderByDescending(d => d.DepartmentName),
                "DepartmentCode" => sortOrder == "asc"
                    ? query.OrderBy(d => d.DepartmentCode)
                    : query.OrderByDescending(d => d.DepartmentCode),
                _ => query.OrderBy(d => d.DepartmentName)
            };

            // Toplam sayı ve sayfa hesaplama
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Pagination
            var departments = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(departments);
        }

        public async Task<IActionResult> Create(int? companyId)
        {
            if (companyId.HasValue)
            {
                ViewBag.CompanyId = companyId;
            }
            else
            {
               // Tüm şirketleri listeye ekle (Dropdown için)
               ViewBag.Companies = await _companyService.GetAllAsync();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            if (ModelState.IsValid)
            {
                await _departmentService.CreateAsync(department);
                AddSuccessMessage(T("department.createdSuccess"));
                return RedirectToAction(nameof(Index), new { companyId = department.CompanyID });
            }
             ViewBag.Companies = await _companyService.GetAllAsync();
            return View(department);
        }

        public async Task<IActionResult> Details(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                AddErrorMessage(T("department.notFound"));
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                AddErrorMessage(T("department.notFound"));
                return RedirectToAction(nameof(Index));
            }

            var companies = await _companyService.GetAllAsync();
            ViewBag.Companies = companies.Where(c => c.IsActive);
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            if (id != department.DepartmentID)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _departmentService.UpdateAsync(department);
                AddSuccessMessage(T("department.updatedSuccess"));
                return RedirectToAction(nameof(Index), new { companyId = department.CompanyID });
            }

            var companies = await _companyService.GetAllAsync();
            ViewBag.Companies = companies.Where(c => c.IsActive);
            return View(department);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                AddErrorMessage(T("department.notFound"));
                return RedirectToAction(nameof(Index));
            }
            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            if (department != null)
            {
                var companyId = department.CompanyID;
                await _departmentService.DeleteAsync(id);
                AddSuccessMessage(T("department.deletedSuccess"));
                return RedirectToAction(nameof(Index), new { companyId });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
