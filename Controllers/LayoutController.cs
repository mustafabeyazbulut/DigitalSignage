using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DigitalSignage.Controllers
{
    public class LayoutController : BaseController
    {
        private readonly ILayoutService _layoutService;
        private readonly ICompanyService _companyService;
        private readonly ITenantContext _tenantContext;

        public LayoutController(
            ILayoutService layoutService,
            ICompanyService companyService,
            ITenantContext tenantContext)
        {
            _layoutService = layoutService;
            _companyService = companyService;
            _tenantContext = tenantContext;
        }

        public async Task<IActionResult> Index(int? companyId)
        {
            // Eğer companyId belirtilmemişse, session'dan veya ilk şirketi al
            if (!companyId.HasValue)
            {
                companyId = HttpContext.Session.GetInt32("SelectedCompanyId");

                // Session'da da yoksa, ilk aktif şirketi al
                if (!companyId.HasValue)
                {
                    var companies = await _companyService.GetAllAsync();
                    var firstCompany = companies.FirstOrDefault(c => c.IsActive);
                    companyId = firstCompany?.CompanyID ?? 0;

                    if (companyId > 0)
                    {
                        HttpContext.Session.SetInt32("SelectedCompanyId", companyId.Value);
                    }
                }
            }
            else
            {
                // Yeni companyId seçilmişse session'a kaydet
                HttpContext.Session.SetInt32("SelectedCompanyId", companyId.Value);
            }

            // Şirket listesini ViewBag'e ekle (şirket değiştirici için)
            var allCompanies = await _companyService.GetAllAsync();
            ViewBag.Companies = new SelectList(allCompanies.Where(c => c.IsActive), "CompanyID", "CompanyName", companyId);
            ViewBag.SelectedCompanyId = companyId;

            // Seçili şirketin layout'larını getir
            if (companyId.HasValue && companyId.Value > 0)
            {
                var layouts = await _layoutService.GetByCompanyIdAsync(companyId.Value);
                return View(layouts);
            }

            return View(new List<Layout>());
        }

        public async Task<IActionResult> Create(int? companyId)
        {
            // Eğer companyId belirtilmemişse session'dan al
            if (!companyId.HasValue)
            {
                companyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            }

            var companies = await _companyService.GetAllAsync();
            ViewBag.Companies = new SelectList(companies.Where(c => c.IsActive), "CompanyID", "CompanyName", companyId);

            var layout = new Layout
            {
                CompanyID = companyId ?? 0,
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
            if (ModelState.IsValid)
            {
                layout.CreatedDate = DateTime.UtcNow;
                await _layoutService.CreateAsync(layout);
                AddSuccessMessage(T("layout.createdSuccess"));
                return RedirectToAction(nameof(Index), new { companyId = layout.CompanyID });
            }

            var companies = await _companyService.GetAllAsync();
            ViewBag.Companies = new SelectList(companies.Where(c => c.IsActive), "CompanyID", "CompanyName", layout.CompanyID);
            return View(layout);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var layout = await _layoutService.GetByIdAsync(id);
            if (layout == null) return NotFound();

            var companies = await _companyService.GetAllAsync();
            ViewBag.Companies = new SelectList(companies.Where(c => c.IsActive), "CompanyID", "CompanyName", layout.CompanyID);

            return View(layout);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Layout layout)
        {
            if (id != layout.LayoutID)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _layoutService.UpdateAsync(layout);
                AddSuccessMessage(T("layout.updatedSuccess"));
                return RedirectToAction(nameof(Index), new { companyId = layout.CompanyID });
            }

            var companies = await _companyService.GetAllAsync();
            ViewBag.Companies = new SelectList(companies.Where(c => c.IsActive), "CompanyID", "CompanyName", layout.CompanyID);
            return View(layout);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var layout = await _layoutService.GetByIdAsync(id);
            if (layout == null)
            {
                AddErrorMessage(T("layout.notFound"));
                return RedirectToAction(nameof(Index));
            }
            return View(layout);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var layout = await _layoutService.GetByIdAsync(id);
            if (layout != null)
            {
                var companyId = layout.CompanyID;
                await _layoutService.DeleteAsync(id);
                AddSuccessMessage(T("layout.deletedSuccess"));
                return RedirectToAction(nameof(Index), new { companyId });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
