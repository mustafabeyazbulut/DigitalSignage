using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Controllers
{
    public class ScheduleController : BaseController
    {
        private readonly IScheduleService _scheduleService;
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;
        private readonly IPageService _pageService;

        public ScheduleController(IScheduleService scheduleService, IDepartmentService departmentService, ICompanyService companyService, IPageService pageService)
        {
            _scheduleService = scheduleService;
            _departmentService = departmentService;
            _companyService = companyService;
            _pageService = pageService;
        }

        public async Task<IActionResult> Index(int? departmentId, string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            const int pageSize = 10;

            // Departmana göre veya tüm zamanlamaları al
            IEnumerable<Schedule> allSchedules;
            if (departmentId.HasValue)
            {
                allSchedules = await _scheduleService.GetByDepartmentIdAsync(departmentId.Value);
                ViewBag.DepartmentId = departmentId;
            }
            else
            {
                allSchedules = await _scheduleService.GetAllAsync();
            }

            IEnumerable<Schedule> query = allSchedules;

            // Arama filtresi
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(s =>
                    (s.ScheduleName != null && s.ScheduleName.ToLower().Contains(search))
                );
            }

            // Sıralama
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

            // Toplam sayı ve sayfa hesaplama
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Pagination
            var schedules = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(schedules);
        }

        public async Task<IActionResult> Create(int? departmentId)
        {
            if (departmentId.HasValue)
            {
                ViewBag.DepartmentId = departmentId;
            }
            else
            {
                 ViewBag.Departments = await _departmentService.GetAllAsync();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            if (ModelState.IsValid)
            {
                await _scheduleService.CreateAsync(schedule);
                AddSuccessMessage(T("schedule.createdSuccess"));
                return RedirectToAction(nameof(Index), new { departmentId = schedule.DepartmentID });
            }
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(schedule);
        }

        public async Task<IActionResult> Details(int id)
        {
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(schedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Schedule schedule)
        {
            if (id != schedule.ScheduleID)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _scheduleService.UpdateAsync(schedule);
                AddSuccessMessage(T("schedule.updatedSuccess"));
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(schedule);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null)
            {
                AddErrorMessage(T("schedule.notFound"));
                return RedirectToAction(nameof(Index));
            }
            return View(schedule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule != null)
            {
                await _scheduleService.DeleteAsync(id);
                AddSuccessMessage(T("schedule.deletedSuccess"));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
