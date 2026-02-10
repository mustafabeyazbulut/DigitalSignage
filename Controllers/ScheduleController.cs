using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Controllers
{
    public class ScheduleController : BaseController
    {
        private readonly IScheduleService _scheduleService;
        private readonly IDepartmentService _departmentService;

        public ScheduleController(IScheduleService scheduleService, IDepartmentService departmentService)
        {
            _scheduleService = scheduleService;
            _departmentService = departmentService;
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
                AddSuccessMessage("Schedule created successfully.");
                return RedirectToAction(nameof(Index), new { departmentId = schedule.DepartmentID });
            }
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(schedule);
        }
    }
}
