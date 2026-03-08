using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthService = DigitalSignage.Services.IAuthorizationService;

namespace DigitalSignage.Controllers
{
    public class ScheduleController : BaseController
    {
        private readonly IScheduleService _scheduleService;
        private readonly ICompanyService _companyService;
        private readonly IPageService _pageService;
        private readonly IDepartmentService _departmentService;
        private readonly AuthService _authService;

        public ScheduleController(
            IScheduleService scheduleService,
            ICompanyService companyService,
            IPageService pageService,
            IDepartmentService departmentService,
            AuthService authService)
        {
            _scheduleService = scheduleService;
            _companyService = companyService;
            _pageService = pageService;
            _departmentService = departmentService;
            _authService = authService;
        }

        // Sadece SystemAdmin veya CompanyAdmin erişebilir
        private async Task<bool> CanAccessSchedulesAsync(int userId, int companyId)
        {
            if (await _authService.IsSystemAdminAsync(userId))
                return true;
            return await _authService.IsCompanyAdminAsync(userId, companyId);
        }

        public async Task<IActionResult> Index(string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            const int pageSize = 10;
            var userId = GetCurrentUserId();
            var isSystemAdmin = await _authService.IsSystemAdminAsync(userId);

            // Şirket context'ini al
            var companyId = HttpContext.Session.GetInt32("SelectedCompanyId");

            // SystemAdmin veya CompanyAdmin olmalı
            if (!isSystemAdmin && !await _authService.HasAnyCompanyAdminRoleAsync(userId))
                return AccessDenied();

            IEnumerable<Schedule> allSchedules;

            if (companyId.HasValue && companyId.Value > 0)
            {
                if (!await CanAccessSchedulesAsync(userId, companyId.Value))
                    return AccessDenied();

                allSchedules = await _scheduleService.GetByCompanyIdAsync(companyId.Value);
            }
            else if (isSystemAdmin)
            {
                // SystemAdmin şirket seçmeden tüm schedule'ları görebilir
                allSchedules = await _scheduleService.GetAllAsync();
            }
            else
            {
                // CompanyAdmin ise kendi şirketlerindeki schedule'ları göster
                var companies = await _authService.GetUserCompaniesAsync(userId);
                var adminCompanies = new List<Company>();
                foreach (var c in companies)
                {
                    if (await _authService.IsCompanyAdminAsync(userId, c.CompanyID))
                        adminCompanies.Add(c);
                }

                var scheduleList = new List<Schedule>();
                foreach (var c in adminCompanies)
                {
                    var companySchedules = await _scheduleService.GetByCompanyIdAsync(c.CompanyID);
                    scheduleList.AddRange(companySchedules);
                }
                allSchedules = scheduleList;
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
            ViewBag.CanEdit = true; // Bu sayfaya sadece admin erişebildiği için her zaman true
            ViewBag.CanDelete = true;

            return View(schedules);
        }

        public async Task<IActionResult> Create()
        {
            var userId = GetCurrentUserId();
            var companyId = HttpContext.Session.GetInt32("SelectedCompanyId");

            if (!companyId.HasValue || companyId.Value <= 0)
            {
                AddErrorMessage(T("common.selectCompanyFirst"));
                return RedirectToAction(nameof(Index));
            }

            if (!await CanAccessSchedulesAsync(userId, companyId.Value))
                return AccessDenied();

            // Şirketin tüm sayfalarını getir
            ViewBag.CompanyId = companyId.Value;
            ViewBag.Pages = await GetCompanyPagesAsync(companyId.Value);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule, int[] selectedPageIds, int[] pageDurations)
        {
            var userId = GetCurrentUserId();

            if (!await CanAccessSchedulesAsync(userId, schedule.CompanyID))
                return AccessDenied();

            ModelState.Remove("Company");
            ModelState.Remove("SchedulePages");

            if (ModelState.IsValid)
            {
                await _scheduleService.CreateAsync(schedule);

                // Seçili sayfaları ekle
                if (selectedPageIds != null && selectedPageIds.Length > 0)
                {
                    for (int i = 0; i < selectedPageIds.Length; i++)
                    {
                        var duration = (pageDurations != null && i < pageDurations.Length) ? pageDurations[i] : 30;
                        await _scheduleService.AddPageToScheduleAsync(schedule.ScheduleID, selectedPageIds[i], duration, i + 1);
                    }
                }

                AddSuccessMessage(T("schedule.createdSuccess"));
                return RedirectToAction(nameof(Edit), new { id = schedule.ScheduleID });
            }

            ViewBag.CompanyId = schedule.CompanyID;
            ViewBag.Pages = await GetCompanyPagesAsync(schedule.CompanyID);
            return View(schedule);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetScheduleWithPagesAsync(id);

            if (schedule == null)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await CanAccessSchedulesAsync(userId, schedule.CompanyID))
                return AccessDenied();

            return View(schedule);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetScheduleWithPagesAsync(id);

            if (schedule == null)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await CanAccessSchedulesAsync(userId, schedule.CompanyID))
                return AccessDenied();

            ViewBag.CompanyId = schedule.CompanyID;
            ViewBag.Pages = await GetCompanyPagesAsync(schedule.CompanyID);

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

            if (!await CanAccessSchedulesAsync(userId, schedule.CompanyID))
                return AccessDenied();

            ModelState.Remove("Company");
            ModelState.Remove("SchedulePages");

            if (ModelState.IsValid)
            {
                await _scheduleService.UpdateAsync(schedule);
                AddSuccessMessage(T("schedule.updatedSuccess"));
                return RedirectToAction(nameof(Edit), new { id = schedule.ScheduleID });
            }

            ViewBag.CompanyId = schedule.CompanyID;
            ViewBag.Pages = await GetCompanyPagesAsync(schedule.CompanyID);
            return View(schedule);
        }

        // AJAX: Sayfa ekle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPage(int scheduleId, int pageId, int duration = 30)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetByIdAsync(scheduleId);
            if (schedule == null)
                return NotFound();

            if (!await CanAccessSchedulesAsync(userId, schedule.CompanyID))
                return Forbid();

            // Mevcut sıradaki son elemanın order'ını al
            var existing = await _scheduleService.GetScheduleWithPagesAsync(scheduleId);
            var nextOrder = (existing?.SchedulePages?.Count ?? 0) + 1;

            await _scheduleService.AddPageToScheduleAsync(scheduleId, pageId, duration, nextOrder);
            return Ok();
        }

        // AJAX: Sayfa kaldır
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePage(int schedulePageId, int scheduleId)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetByIdAsync(scheduleId);
            if (schedule == null)
                return NotFound();

            if (!await CanAccessSchedulesAsync(userId, schedule.CompanyID))
                return Forbid();

            await _scheduleService.RemovePageFromScheduleAsync(schedulePageId);
            return Ok();
        }

        // AJAX: Süre güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePageDuration(int schedulePageId, int scheduleId, int duration)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetByIdAsync(scheduleId);
            if (schedule == null)
                return NotFound();

            if (!await CanAccessSchedulesAsync(userId, schedule.CompanyID))
                return Forbid();

            var sp = await _scheduleService.GetScheduleWithPagesAsync(scheduleId);
            var item = sp?.SchedulePages?.FirstOrDefault(x => x.SchedulePageID == schedulePageId);
            if (item == null)
                return NotFound();

            await _scheduleService.UpdateSchedulePageAsync(schedulePageId, duration, item.DisplayOrder);
            return Ok();
        }

        // AJAX: Sıralama güncelle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderPages(int scheduleId, [FromBody] int[] schedulePageIds)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetByIdAsync(scheduleId);
            if (schedule == null)
                return NotFound();

            if (!await CanAccessSchedulesAsync(userId, schedule.CompanyID))
                return Forbid();

            await _scheduleService.ReorderSchedulePagesAsync(scheduleId, schedulePageIds);
            return Ok();
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var schedule = await _scheduleService.GetScheduleWithPagesAsync(id);

            if (schedule == null)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await CanAccessSchedulesAsync(userId, schedule.CompanyID))
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

            if (!await CanAccessSchedulesAsync(userId, schedule.CompanyID))
                return AccessDenied();

            await _scheduleService.DeleteAsync(id);
            AddSuccessMessage(T("schedule.deletedSuccess"));
            return RedirectToAction(nameof(Index));
        }

        // Şirketin tüm aktif sayfalarını departman bilgisiyle getir
        private async Task<List<Page>> GetCompanyPagesAsync(int companyId)
        {
            var departments = await _departmentService.GetByCompanyIdAsync(companyId);
            var allPages = new List<Page>();

            foreach (var dept in departments)
            {
                var deptPages = await _pageService.GetActivePagesByDepartmentAsync(dept.DepartmentID);
                foreach (var p in deptPages)
                {
                    p.Department = dept; // Navigation property'yi set et
                }
                allPages.AddRange(deptPages);
            }

            return allPages
                .OrderBy(p => p.Department?.DepartmentName)
                .ThenBy(p => p.PageName)
                .ToList();
        }
    }
}
