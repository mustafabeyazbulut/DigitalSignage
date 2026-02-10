using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Controllers
{
    public class LayoutController : BaseController
    {
        private readonly ILayoutService _layoutService;
        private readonly ICompanyService _companyService;

        public LayoutController(ILayoutService layoutService, ICompanyService companyService)
        {
            _layoutService = layoutService;
            _companyService = companyService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _layoutService.GetAllAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Companies = await _companyService.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Layout layout)
        {
            if (ModelState.IsValid)
            {
                // Layout Section'ları otomatik oluşturma mantığı burada olabilir
                // Veya client-side'dan JSON olarak gelip deserialize edilebilir.
                
                await _layoutService.CreateAsync(layout);
                AddSuccessMessage("Layout created successfully.");
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Companies = await _companyService.GetAllAsync();
            return View(layout);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var layout = await _layoutService.GetByIdAsync(id);
            if (layout == null) return NotFound();
            return View(layout);
        }
    }
}
