using System.Text.Json;
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

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var layout = await _layoutService.GetLayoutWithSectionsAsync(id);

            if (layout == null)
            {
                AddErrorMessage(T("layout.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.CanAccessCompanyAsync(userId, layout.CompanyID))
                return AccessDenied();

            return View(layout);
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
                LayoutDefinition = "{\"rows\":[{\"height\":50,\"columns\":[{\"width\":50},{\"width\":50}]},{\"height\":50,\"columns\":[{\"width\":50},{\"width\":50}]}]}",
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

            // Form'da gelmeyen non-nullable property'leri ModelState'ten çıkar
            ModelState.Remove("Company");
            ModelState.Remove("LayoutSections");
            ModelState.Remove("Pages");

            // LayoutDefinition JSON doğrulaması
            if (!IsValidLayoutDefinition(layout.LayoutDefinition))
            {
                ModelState.AddModelError("LayoutDefinition", T("layout.invalidDefinition"));
            }

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

            // Form'da gelmeyen non-nullable property'leri ModelState'ten çıkar
            ModelState.Remove("Company");
            ModelState.Remove("LayoutSections");
            ModelState.Remove("Pages");

            // LayoutDefinition JSON doğrulaması
            if (!IsValidLayoutDefinition(layout.LayoutDefinition))
            {
                ModelState.AddModelError("LayoutDefinition", T("layout.invalidDefinition"));
            }

            if (ModelState.IsValid)
            {
                // Düzen tanımını güncelle ve bölümleri yeniden oluştur
                await _layoutService.UpdateLayoutDefinitionAsync(layout.LayoutID, layout.LayoutDefinition);

                // Diğer alanları güncelle
                var existingLayout = await _layoutService.GetByIdAsync(id);
                if (existingLayout != null)
                {
                    existingLayout.LayoutName = layout.LayoutName;
                    existingLayout.Description = layout.Description;
                    existingLayout.IsActive = layout.IsActive;
                    await _layoutService.UpdateAsync(existingLayout);
                }

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

            ViewBag.IsInUse = await _layoutService.IsLayoutInUseAsync(id);
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

        private bool IsValidLayoutDefinition(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var def = JsonSerializer.Deserialize<LayoutDefinitionModel>(json, options);
                if (def?.Rows == null || def.Rows.Count == 0) return false;

                return ValidateRows(def.Rows);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Satır/sütun yapısını recursive olarak doğrular.
        /// İç içe sütunların alt satırları da aynı kurallara tabi.
        /// </summary>
        private static bool ValidateRows(List<LayoutRowDefinition> rows)
        {
            double totalHeight = 0;
            foreach (var row in rows)
            {
                if (row.Columns == null || row.Columns.Count == 0) return false;
                if (row.Height <= 0) return false;
                totalHeight += row.Height;

                double totalWidth = 0;
                foreach (var col in row.Columns)
                {
                    if (col.Width <= 0) return false;
                    totalWidth += col.Width;

                    // İç içe bölünmüş sütunun alt satırlarını da doğrula
                    if (col.Rows != null && col.Rows.Count > 0)
                    {
                        if (!ValidateRows(col.Rows)) return false;
                    }
                }
                if (Math.Abs(totalWidth - 100) > 0.5) return false;
            }
            if (Math.Abs(totalHeight - 100) > 0.5) return false;

            return true;
        }
    }
}
