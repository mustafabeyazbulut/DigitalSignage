using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace DigitalSignage.Controllers
{
    /// <summary>
    /// Dijital tabela görüntüleme controller'ı — herkese açık, tam ekran.
    /// </summary>
    [AllowAnonymous]
    public class DisplayController : Controller
    {
        private readonly IPageService _pageService;
        private readonly IScheduleService _scheduleService;
        private readonly IFileStorageService _fileStorage;

        public DisplayController(IPageService pageService, IScheduleService scheduleService, IFileStorageService fileStorage)
        {
            _pageService = pageService;
            _scheduleService = scheduleService;
            _fileStorage = fileStorage;
        }

        /// <summary>
        /// Sayfayı tam ekran görüntüle — anonim erişim.
        /// </summary>
        [HttpGet("Display/{id}")]
        public async Task<IActionResult> View(int id)
        {
            var page = await _pageService.GetPageFullDetailsAsync(id);
            if (page == null || !page.IsActive)
                return NotFound();

            // Section içerik haritasını al
            var contentMap = await _pageService.GetSectionContentMapAsync(id);
            ViewBag.ContentMap = contentMap;

            return View(page);
        }

        /// <summary>
        /// Zamanlama görüntüleme — sayfalar arası geçişli tam ekran.
        /// </summary>
        [HttpGet("Display/Schedule/{id}")]
        public async Task<IActionResult> Schedule(int id)
        {
            var schedule = await _scheduleService.GetScheduleWithPagesAsync(id);
            if (schedule == null || !schedule.IsActive)
                return NotFound();

            return View(schedule);
        }

        /// <summary>
        /// Zamanlama verilerini JSON olarak döndür — döngü sonunda güncelleme için.
        /// </summary>
        [HttpGet("Display/ScheduleData/{id}")]
        public async Task<IActionResult> ScheduleData(int id)
        {
            var schedule = await _scheduleService.GetScheduleWithPagesAsync(id);
            if (schedule == null || !schedule.IsActive)
                return NotFound();

            var pages = schedule.SchedulePages
                .Where(sp => sp.IsActive && sp.Page != null && sp.Page.IsActive)
                .OrderBy(sp => sp.DisplayOrder)
                .Select(sp => new
                {
                    pageId = sp.PageID,
                    pageName = sp.Page!.PageTitle,
                    duration = sp.DisplayDuration,
                    order = sp.DisplayOrder
                })
                .ToList();

            return Json(new
            {
                scheduleId = schedule.ScheduleID,
                scheduleName = schedule.ScheduleName,
                pages
            });
        }

        /// <summary>
        /// Display sayfası için anonim dosya erişimi (medya ve logo).
        /// </summary>
        [HttpGet("display-media/{companyId}/{fileName}")]
        public IActionResult ServeMedia(int companyId, string fileName)
        {
            var mediaPath = $"{companyId}/{fileName}";
            var physicalPath = _fileStorage.GetPhysicalPath(mediaPath);

            if (!System.IO.File.Exists(physicalPath))
                return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
                contentType = "application/octet-stream";

            return PhysicalFile(physicalPath, contentType);
        }
    }
}
