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

        public async Task<IActionResult> Index(int? departmentId)
        {
             if (departmentId.HasValue)
            {
                ViewBag.DepartmentId = departmentId;
                return View(await _scheduleService.GetByDepartmentIdAsync(departmentId.Value));
            }
            return View(await _scheduleService.GetAllAsync());
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
