using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;
using AuthService = DigitalSignage.Services.IAuthorizationService;

namespace DigitalSignage.Controllers
{
    public class DepartmentController : BaseController
    {
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;
        private readonly AuthService _authService;

        public DepartmentController(IDepartmentService departmentService, ICompanyService companyService, AuthService authService)
        {
            _departmentService = departmentService;
            _companyService = companyService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(int? companyId, string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            const int pageSize = 10;
            var userId = GetCurrentUserId();
            if (!await _authService.HasAnyRoleAsync(userId))
                return AccessDenied();
            var isSystemAdmin = await _authService.IsSystemAdminAsync(userId);

            // Session'dan seçili şirketi al (şirket değiştirme bağlamı)
            if (!companyId.HasValue)
            {
                var sessionCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
                if (sessionCompanyId.HasValue && sessionCompanyId.Value > 0)
                    companyId = sessionCompanyId.Value;
            }

            IEnumerable<Department> allDepartments;

            if (companyId.HasValue)
            {
                // Kullanıcının bu şirkete erişimi var mı?
                if (!await _authService.CanAccessCompanyAsync(userId, companyId.Value))
                    return AccessDenied();

                if (isSystemAdmin || await _authService.IsCompanyAdminAsync(userId, companyId.Value))
                {
                    // SystemAdmin ve CompanyAdmin tüm departmanları görür
                    allDepartments = await _departmentService.GetByCompanyIdAsync(companyId.Value);
                }
                else
                {
                    // Diğer roller sadece erişebildiği departmanları görür
                    allDepartments = await _authService.GetUserDepartmentsAsync(userId, companyId.Value);
                }
                ViewBag.CompanyId = companyId;
            }
            else if (isSystemAdmin)
            {
                allDepartments = await _departmentService.GetAllAsync();
            }
            else
            {
                // Kullanıcının erişebildiği şirketlerin departmanlarını getir
                var userCompanies = await _authService.GetUserCompaniesAsync(userId);
                var deptList = new List<Department>();
                foreach (var company in userCompanies)
                {
                    var depts = await _authService.GetUserDepartmentsAsync(userId, company.CompanyID);
                    deptList.AddRange(depts);
                }
                allDepartments = deptList;
            }

            IEnumerable<Department> query = allDepartments;

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(d =>
                    (d.DepartmentName != null && d.DepartmentName.ToLower().Contains(search)) ||
                    (d.DepartmentCode != null && d.DepartmentCode.ToLower().Contains(search)) ||
                    (d.Company?.CompanyName != null && d.Company.CompanyName.ToLower().Contains(search))
                );
            }

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

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var departments = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(departments);
        }

        public async Task<IActionResult> Create(int? companyId)
        {
            var userId = GetCurrentUserId();
            if (!await _authService.HasAnyRoleAsync(userId))
                return AccessDenied();

            if (companyId.HasValue)
            {
                // O şirketin CompanyAdmin'i veya SystemAdmin olmalı
                if (!await _authService.IsCompanyAdminAsync(userId, companyId.Value))
                    return AccessDenied();

                ViewBag.CompanyId = companyId;
            }
            else
            {
                if (!await _authService.IsSystemAdminAsync(userId) &&
                    !await _authService.HasAnyCompanyAdminRoleAsync(userId))
                    return AccessDenied();

                ViewBag.Companies = await _authService.GetUserCompaniesAsync(userId);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department department)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.IsCompanyAdminAsync(userId, department.CompanyID))
                return AccessDenied();

            // Form'da gelmeyen non-nullable property'leri ModelState'ten çıkar
            ModelState.Remove("Company");
            ModelState.Remove("Pages");
            ModelState.Remove("Schedules");
            ModelState.Remove("Contents");
            ModelState.Remove("UserDepartmentRoles");
            ModelState.Remove("CreatedBy");

            if (ModelState.IsValid)
            {
                await _departmentService.CreateAsync(department);
                AddSuccessMessage(T("department.createdSuccess"));
                return RedirectToAction(nameof(Index), new { companyId = department.CompanyID });
            }

            ViewBag.Companies = await _authService.GetUserCompaniesAsync(userId);
            return View(department);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanAccessDepartmentAsync(userId, id))
                return AccessDenied();

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
            var userId = GetCurrentUserId();

            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                AddErrorMessage(T("department.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.IsCompanyAdminAsync(userId, department.CompanyID))
                return AccessDenied();

            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Department department)
        {
            var userId = GetCurrentUserId();

            if (id != department.DepartmentID)
            {
                AddErrorMessage(T("department.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.IsCompanyAdminAsync(userId, department.CompanyID))
                return AccessDenied();

            // Form'da gelmeyen non-nullable property'leri ModelState'ten çıkar
            ModelState.Remove("Company");
            ModelState.Remove("Pages");
            ModelState.Remove("Schedules");
            ModelState.Remove("Contents");
            ModelState.Remove("UserDepartmentRoles");
            ModelState.Remove("CreatedBy");

            if (ModelState.IsValid)
            {
                await _departmentService.UpdateAsync(department);
                AddSuccessMessage(T("department.updatedSuccess"));
                return RedirectToAction(nameof(Index), new { companyId = department.CompanyID });
            }

            return View(department);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                AddErrorMessage(T("department.notFound"));
                return RedirectToAction(nameof(Index));
            }

            // Departman silme: CompanyAdmin (SystemAdmin dahil) yetkisi gerekli
            if (!await _authService.IsCompanyAdminAsync(userId, department.CompanyID))
                return AccessDenied();

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            var department = await _departmentService.GetByIdAsync(id);
            if (department == null)
            {
                AddErrorMessage(T("department.notFound"));
                return RedirectToAction(nameof(Index));
            }

            // Departman silme: CompanyAdmin (SystemAdmin dahil) yetkisi gerekli
            if (!await _authService.IsCompanyAdminAsync(userId, department.CompanyID))
                return AccessDenied();

            var companyId = department.CompanyID;
            await _departmentService.DeleteAsync(id);
            AddSuccessMessage(T("department.deletedSuccess"));
            return RedirectToAction(nameof(Index), new { companyId });
        }
    }
}
