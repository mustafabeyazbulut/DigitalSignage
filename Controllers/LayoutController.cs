using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;
using AuthService = DigitalSignage.Services.IAuthorizationService;

namespace DigitalSignage.Controllers
{
    public class LayoutController : BaseController
    {
        private readonly ILayoutService _layoutService;
        private readonly ICompanyService _companyService;
        private readonly IDepartmentService _departmentService;
        private readonly ITenantContext _tenantContext;
        private readonly AuthService _authService;

        public LayoutController(
            ILayoutService layoutService,
            ICompanyService companyService,
            IDepartmentService departmentService,
            ITenantContext tenantContext,
            AuthService authService)
        {
            _layoutService = layoutService;
            _companyService = companyService;
            _departmentService = departmentService;
            _tenantContext = tenantContext;
            _authService = authService;
        }

        public async Task<IActionResult> Index(int? companyId, int? departmentId)
        {
            var userId = GetCurrentUserId();
            if (!await _authService.HasAnyRoleAsync(userId))
                return AccessDenied();

            // Departman bağlamı varsa, o departmanın şirketini kullan
            if (departmentId.HasValue)
            {
                var dept = await _departmentService.GetByIdAsync(departmentId.Value);
                if (dept == null || !await _authService.CanAccessDepartmentAsync(userId, departmentId.Value))
                    return AccessDenied();

                companyId = dept.CompanyID;
                ViewBag.DepartmentContext = dept;
            }
            else if (!companyId.HasValue)
            {
                companyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            }

            // Şirket belirlenmiş ise erişim kontrolü yap
            if (companyId.HasValue && companyId.Value > 0)
            {
                if (!await _authService.CanAccessCompanyAsync(userId, companyId.Value))
                    return AccessDenied();
            }

            if (companyId.HasValue && companyId.Value > 0)
            {
                var layouts = await _layoutService.GetByCompanyIdAsync(companyId.Value);
                return View(layouts);
            }

            return View(new List<Layout>());
        }

        public async Task<IActionResult> Create(int? companyId)
        {
            var userId = GetCurrentUserId();
            if (!await _authService.HasAnyRoleAsync(userId))
                return AccessDenied();

            if (!companyId.HasValue)
            {
                companyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            }

            // Şirket seçili değilse erişim reddedilir
            if (!companyId.HasValue || companyId.Value <= 0)
            {
                AddErrorMessage(T("common.selectCompanyFirst"));
                return RedirectToAction(nameof(Index));
            }

            // Layout oluşturma için CompanyAdmin veya SystemAdmin gerekli
            if (!await _authService.IsCompanyAdminAsync(userId, companyId.Value))
                return AccessDenied();

            var layout = new Layout
            {
                CompanyID = companyId.Value,
                GridColumnsX = 2,
                GridRowsY = 2,
                IsActive = true
            };

            return View(layout);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Layout layout)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.IsCompanyAdminAsync(userId, layout.CompanyID))
                return AccessDenied();

            if (ModelState.IsValid)
            {
                layout.CreatedDate = DateTime.UtcNow;
                await _layoutService.CreateAsync(layout);
                AddSuccessMessage(T("layout.createdSuccess"));
                return RedirectToAction(nameof(Index), new { companyId = layout.CompanyID });
            }

            return View(layout);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var layout = await _layoutService.GetByIdAsync(id);

            if (layout == null)
            {
                AddErrorMessage(T("layout.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.IsCompanyAdminAsync(userId, layout.CompanyID))
                return AccessDenied();

            return View(layout);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Layout layout)
        {
            var userId = GetCurrentUserId();

            if (id != layout.LayoutID)
            {
                AddErrorMessage(T("layout.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.IsCompanyAdminAsync(userId, layout.CompanyID))
                return AccessDenied();

            if (ModelState.IsValid)
            {
                await _layoutService.UpdateAsync(layout);
                AddSuccessMessage(T("layout.updatedSuccess"));
                return RedirectToAction(nameof(Index), new { companyId = layout.CompanyID });
            }

            return View(layout);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var layout = await _layoutService.GetByIdAsync(id);

            if (layout == null)
            {
                AddErrorMessage(T("layout.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.IsCompanyAdminAsync(userId, layout.CompanyID))
                return AccessDenied();

            return View(layout);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            var layout = await _layoutService.GetByIdAsync(id);

            if (layout == null)
            {
                AddErrorMessage(T("layout.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.IsCompanyAdminAsync(userId, layout.CompanyID))
                return AccessDenied();

            var companyId = layout.CompanyID;
            await _layoutService.DeleteAsync(id);
            AddSuccessMessage(T("layout.deletedSuccess"));
            return RedirectToAction(nameof(Index), new { companyId });
        }
    }
}
