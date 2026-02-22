using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;
using AuthService = DigitalSignage.Services.IAuthorizationService;

namespace DigitalSignage.Controllers
{
    public class ScheduleController : BaseController
    {
        private readonly IScheduleService _scheduleService;
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;
        private readonly IPageService _pageService;
        private readonly AuthService _authService;

        public ScheduleController(
            IScheduleService scheduleService,
            IDepartmentService departmentService,
            ICompanyService companyService,
            IPageService pageService,
            AuthService authService)
        {
            _scheduleService = scheduleService;
            _departmentService = departmentService;
            _companyService = companyService;
            _pageService = pageService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(int? departmentId, string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            const int pageSize = 10;
            var userId = GetCurrentUserId();
            if (!await _authService.HasAnyRoleAsync(userId))
                return AccessDenied();

            IEnumerable<Schedule> allSchedules;

            if (departmentId.HasValue)
            {
                if (!await _authService.CanAccessDepartmentAsync(userId, departmentId.Value))
                    return AccessDenied();

                allSchedules = await _scheduleService.GetByDepartmentIdAsync(departmentId.Value);
                ViewBag.DepartmentId = departmentId;

                ViewBag.CanEdit = await _authService.CanEditInDepartmentAsync(userId, departmentId.Value);
                ViewBag.CanDelete = await _authService.IsDepartmentManagerAsync(userId, departmentId.Value);
            }
            else
            {
                var sessionCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
                var isSystemAdmin = await _authService.IsSystemAdminAsync(userId);

                if (isSystemAdmin && (!sessionCompanyId.HasValue || sessionCompanyId.Value <= 0))
                {
                    allSchedules = await _scheduleService.GetAllAsync();
                }
                else
                {
                    List<Models.Company> companies;
                    if (sessionCompanyId.HasValue && sessionCompanyId.Value > 0)
                    {
                        if (!await _authService.CanAccessCompanyAsync(userId, sessionCompanyId.Value))
                            return AccessDenied();
                        var company = await _companyService.GetByIdAsync(sessionCompanyId.Value);
                        companies = company != null ? new List<Models.Company> { company } : new List<Models.Company>();
                    }
                    else
                    {
                        companies = await _authService.GetUserCompaniesAsync(userId);
                    }

                    var scheduleList = new List<Schedule>();
                    foreach (var company in companies)
                    {
                        var depts = await _authService.GetUserDepartmentsAsync(userId, company.CompanyID);
                        foreach (var dept in depts)
                        {
                            var deptSchedules = await _scheduleService.GetByDepartmentIdAsync(dept.DepartmentID);
                            scheduleList.AddRange(deptSchedules);
                        }
                    }
                    allSchedules = scheduleList;
                }

                ViewBag.CanEdit = isSystemAdmin || await _authService.HasAnyCompanyAdminRoleAsync(userId);
                ViewBag.CanDelete = ViewBag.CanEdit;
            }

            IEnumerable<Schedule> query = allSchedules;

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(s =>
                    (s.ScheduleName != null && s.ScheduleName.ToLower().Contains(search))
                );
            }

            query = sortBy switch
            {
                "ScheduleName" => sortOrder == "asc"
                    ? query.OrderBy(s => s.ScheduleName)
                    : query.OrderByDescending(s => s.ScheduleName),
                "StartDate" => sortOrder == "asc"
                    ? query.OrderBy(s => s.StartDate)
                    : query.OrderByDescending(s => s.StartDate),
                "EndDate" => sortOrder == "asc"
                    ? query.OrderBy(s => s.EndDate)
                    : query.OrderByDescending(s => s.EndDate),
                _ => query.OrderBy(s => s.ScheduleName)
            };

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var schedules = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(schedules);
        }

        public async Task<IActionResult> Create(int? departmentId)
        {
            var userId = GetCurrentUserId();
            if (!await _authService.HasAnyRoleAsync(userId))
                return AccessDenied();

            if (departmentId.HasValue)
            {
                if (!await _authService.CanEditInDepartmentAsync(userId, departmentId.Value))
                    return AccessDenied();

                ViewBag.DepartmentId = departmentId;
            }
            else
            {
                ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanEditInDepartmentAsync(userId, schedule.DepartmentID))
                return AccessDenied();

            // Form'da gelmeyen non-nullable property'leri ModelState'ten çıkar
            ModelState.Remove("Department");
            ModelState.Remove("SchedulePages");

            if (ModelState.IsValid)
            {
                await _scheduleService.CreateAsync(schedule);
                AddSuccessMessage(T("schedule.createdSuccess"));
                return RedirectToAction(nameof(Index), new { departmentId = schedule.DepartmentID });
            }

            ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
            return View(schedule);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);

            if (schedule == null)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.CanAccessDepartmentAsync(userId, schedule.DepartmentID))
                return AccessDenied();

            return View(schedule);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);

            if (schedule == null)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.CanEditInDepartmentAsync(userId, schedule.DepartmentID))
                return AccessDenied();

            ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Schedule schedule)
        {
            var userId = GetCurrentUserId();

            if (id != schedule.ScheduleID)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.CanEditInDepartmentAsync(userId, schedule.DepartmentID))
                return AccessDenied();

            // Form'da gelmeyen non-nullable property'leri ModelState'ten çıkar
            ModelState.Remove("Department");
            ModelState.Remove("SchedulePages");

            if (ModelState.IsValid)
            {
                await _scheduleService.UpdateAsync(schedule);
                AddSuccessMessage(T("schedule.updatedSuccess"));
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
            return View(schedule);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);

            if (schedule == null)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.IsDepartmentManagerAsync(userId, schedule.DepartmentID))
                return AccessDenied();

            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetByIdAsync(id);

            if (schedule == null)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.IsDepartmentManagerAsync(userId, schedule.DepartmentID))
                return AccessDenied();

            await _scheduleService.DeleteAsync(id);
            AddSuccessMessage(T("schedule.deletedSuccess"));
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Department>> GetAccessibleDepartmentsAsync(int userId)
        {
            var sessionCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            var departments = new List<Department>();

            if (sessionCompanyId.HasValue && sessionCompanyId.Value > 0)
            {
                var depts = await _authService.GetUserDepartmentsAsync(userId, sessionCompanyId.Value);
                departments.AddRange(depts);
            }
            else
            {
                var userCompanies = await _authService.GetUserCompaniesAsync(userId);
                foreach (var company in userCompanies)
                {
                    var depts = await _authService.GetUserDepartmentsAsync(userId, company.CompanyID);
                    departments.AddRange(depts);
                }
            }
            return departments;
        }
    }
}
