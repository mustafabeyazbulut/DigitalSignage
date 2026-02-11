using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Controllers
{
    public class PageController : BaseController
    {
        private readonly IPageService _pageService;
        private readonly ILayoutService _layoutService;
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;

        public PageController(IPageService pageService, ILayoutService layoutService, IDepartmentService departmentService, ICompanyService companyService)
        {
            _pageService = pageService;
            _layoutService = layoutService;
            _departmentService = departmentService;
            _companyService = companyService;
        }

        public async Task<IActionResult> Index(int? departmentId)
        {
            if (departmentId.HasValue)
            {
                var pages = await _pageService.GetByDepartmentIdAsync(departmentId.Value);
                ViewBag.DepartmentId = departmentId;
                return View(pages);
            }
            return View(await _pageService.GetAllAsync());
        }

        public async Task<IActionResult> Create(int? departmentId)
        {
            ViewBag.Layouts = await _layoutService.GetAllAsync();
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
        public async Task<IActionResult> Create(Page page)
        {
            if (ModelState.IsValid)
            {
                await _pageService.CreateAsync(page);
                AddSuccessMessage(T("page.createdSuccess"));
                return RedirectToAction(nameof(Index), new { departmentId = page.DepartmentID });
            }
            ViewBag.Layouts = await _layoutService.GetAllAsync();
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(page);
        }

        public async Task<IActionResult> Design(int id)
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page == null) return NotFound();

            // Layout ve PageContent bilgilerini de getir (Service'de Include eklenmeli veya ayrı call yapılmalı)
            // Şimdilik sadece page dönüyoruz.
            return View(page);
        }

        public async Task<IActionResult> Details(int id)
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page == null)
            {
                AddErrorMessage(T("page.notFound"));
                return RedirectToAction(nameof(Index));
            }
            return View(page);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page == null)
            {
                AddErrorMessage(T("page.notFound"));
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = await _departmentService.GetAllAsync();
            ViewBag.Layouts = await _layoutService.GetAllAsync();
            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Page page)
        {
            if (id != page.PageID)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _pageService.UpdateAsync(page);
                AddSuccessMessage(T("page.updatedSuccess"));
                return RedirectToAction(nameof(Index), new { departmentId = page.DepartmentID });
            }

            ViewBag.Departments = await _departmentService.GetAllAsync();
            ViewBag.Layouts = await _layoutService.GetAllAsync();
            return View(page);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page == null)
            {
                AddErrorMessage(T("page.notFound"));
                return RedirectToAction(nameof(Index));
            }
            return View(page);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var page = await _pageService.GetByIdAsync(id);
            if (page != null)
            {
                var departmentId = page.DepartmentID;
                await _pageService.DeleteAsync(id);
                AddSuccessMessage(T("page.deletedSuccess"));
                return RedirectToAction(nameof(Index), new { departmentId });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
