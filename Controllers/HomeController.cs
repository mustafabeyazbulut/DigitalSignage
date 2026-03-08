using System.Diagnostics;
using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ICompanyService _companyService;
        private readonly IDepartmentService _departmentService;
        private readonly ILayoutService _layoutService;
        private readonly IContentService _contentService;
        private readonly IPageService _pageService;
        private readonly IScheduleService _scheduleService;

        public HomeController(
            ICompanyService companyService,
            IDepartmentService departmentService,
            ILayoutService layoutService,
            IContentService contentService,
            IPageService pageService,
            IScheduleService scheduleService)
        {
            _companyService = companyService;
            _departmentService = departmentService;
            _layoutService = layoutService;
            _contentService = contentService;
            _pageService = pageService;
            _scheduleService = scheduleService;
        }

        public async Task<IActionResult> Index()
        {
            var isSystemAdmin = ViewBag.IsSystemAdmin == true;
            var isCompanyAdmin = ViewBag.IsCompanyAdmin == true;

            // SystemAdmin ise toplam şirket sayısını al
            if (isSystemAdmin)
            {
                var companies = await _companyService.GetAllAsync();
                ViewBag.CompanyCount = companies.Count();
            }

            // Seçili şirket varsa şirket bazlı istatistikleri topla
            var companyId = HttpContext.Items["CompanyId"] as int?;
            if (companyId.HasValue)
            {
                var departments = await _departmentService.GetByCompanyIdAsync(companyId.Value);
                var departmentList = departments.ToList();

                ViewBag.DepartmentCount = departmentList.Count;

                // Düzen sayısı (şirket bazlı)
                var layouts = await _layoutService.GetByCompanyIdAsync(companyId.Value);
                ViewBag.LayoutCount = layouts.Count();

                // Zamanlama sayısı (şirket bazlı)
                var schedules = await _scheduleService.GetByCompanyIdAsync(companyId.Value);
                ViewBag.ScheduleCount = schedules.Count();

                // İçerik ve sayfa sayıları (departman bazlı toplanır)
                var contentCount = 0;
                var allPages = new List<Page>();
                foreach (var dept in departmentList)
                {
                    var contents = await _contentService.GetByDepartmentIdAsync(dept.DepartmentID);
                    contentCount += contents.Count();

                    var pages = await _pageService.GetByDepartmentIdAsync(dept.DepartmentID);
                    var pageList = pages.ToList();
                    // Department nav property include edilmediği için manuel ata
                    foreach (var p in pageList) p.Department = dept;
                    allPages.AddRange(pageList);
                }

                ViewBag.ContentCount = contentCount;
                ViewBag.PageCount = allPages.Count;

                // Son eklenen sayfalar (en son 5 sayfa)
                ViewBag.RecentPages = allPages
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(5)
                    .ToList();
            }
            else
            {
                ViewBag.DepartmentCount = 0;
                ViewBag.LayoutCount = 0;
                ViewBag.ScheduleCount = 0;
                ViewBag.ContentCount = 0;
                ViewBag.PageCount = 0;
                ViewBag.RecentPages = new List<Page>();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
