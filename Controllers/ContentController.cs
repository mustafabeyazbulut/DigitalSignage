using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Controllers
{
    public class ContentController : BaseController
    {
        private readonly IContentService _contentService;
        private readonly IDepartmentService _departmentService;

        public ContentController(IContentService contentService, IDepartmentService departmentService)
        {
            _contentService = contentService;
            _departmentService = departmentService;
        }

        public async Task<IActionResult> Index(int? departmentId)
        {
            if (departmentId.HasValue)
            {
                ViewBag.DepartmentId = departmentId;
                return View(await _contentService.GetByDepartmentIdAsync(departmentId.Value));
            }
            return View(await _contentService.GetAllAsync());
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
        public async Task<IActionResult> Create(Content content, IFormFile? mediaFile)
        {
            if (ModelState.IsValid)
            {
                if (mediaFile != null && mediaFile.Length > 0)
                {
                    // Dosya yükleme işlemi
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(mediaFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await mediaFile.CopyToAsync(stream);
                    }
                    
                    content.MediaPath = "/uploads/" + fileName;
                }

                await _contentService.CreateAsync(content);
                AddSuccessMessage("Content uploaded successfully.");
                return RedirectToAction(nameof(Index), new { departmentId = content.DepartmentID });
            }
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(content);
        }
    }
}
