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

        public async Task<IActionResult> Index(int? companyId)
        {
            if (companyId.HasValue)
            {
                var departments = await _departmentService.GetByCompanyIdAsync(companyId.Value);
                ViewBag.CompanyId = companyId;
                return View(departments);
            }
            return View(await _departmentService.GetAllAsync());
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
                AddSuccessMessage("Department created successfully.");
                return RedirectToAction(nameof(Index), new { companyId = department.CompanyID });
            }
             ViewBag.Companies = await _companyService.GetAllAsync();
            return View(department);
        }
    }
}
