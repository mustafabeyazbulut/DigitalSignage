using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;

namespace DigitalSignage.Controllers
{
    public class ContentController : BaseController
    {
        private readonly IContentService _contentService;
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;

        public ContentController(IContentService contentService, IDepartmentService departmentService, ICompanyService companyService)
        {
            _contentService = contentService;
            _departmentService = departmentService;
            _companyService = companyService;
        }

        public async Task<IActionResult> Index(int? departmentId, string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            const int pageSize = 10;

            // Departmana göre veya tüm içerikleri al
            IEnumerable<Content> allContents;
            if (departmentId.HasValue)
            {
                allContents = await _contentService.GetByDepartmentIdAsync(departmentId.Value);
                ViewBag.DepartmentId = departmentId;
            }
            else
            {
                allContents = await _contentService.GetAllAsync();
            }

            IEnumerable<Content> query = allContents;

            // Arama filtresi
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    (c.ContentTitle != null && c.ContentTitle.ToLower().Contains(search)) ||
                    (c.ContentType != null && c.ContentType.ToLower().Contains(search)) ||
                    (c.MediaPath != null && c.MediaPath.ToLower().Contains(search))
                );
            }

            // Sıralama
            query = sortBy switch
            {
                "ContentTitle" => sortOrder == "asc"
                    ? query.OrderBy(c => c.ContentTitle)
                    : query.OrderByDescending(c => c.ContentTitle),
                "ContentType" => sortOrder == "asc"
                    ? query.OrderBy(c => c.ContentType)
                    : query.OrderByDescending(c => c.ContentType),
                _ => query.OrderBy(c => c.ContentTitle)
            };

            // Toplam sayı ve sayfa hesaplama
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Pagination
            var contents = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(contents);
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
                AddSuccessMessage(T("content.createdSuccess"));
                return RedirectToAction(nameof(Index), new { departmentId = content.DepartmentID });
            }
            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(content);
        }

        public async Task<IActionResult> Details(int id)
        {
            var content = await _contentService.GetByIdAsync(id);
            if (content == null)
            {
                AddErrorMessage(T("content.notFound"));
                return RedirectToAction(nameof(Index));
            }
            return View(content);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var content = await _contentService.GetByIdAsync(id);
            if (content == null)
            {
                AddErrorMessage(T("content.notFound"));
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Content content, IFormFile? file)
        {
            if (id != content.ContentID)
                return BadRequest();

            if (ModelState.IsValid)
            {
                // Handle file upload if new file provided
                if (file != null && file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(content.MediaPath))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", content.MediaPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    content.MediaPath = "/uploads/" + fileName;
                }

                await _contentService.UpdateAsync(content);
                AddSuccessMessage(T("content.updatedSuccess"));
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = await _departmentService.GetAllAsync();
            return View(content);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var content = await _contentService.GetByIdAsync(id);
            if (content == null)
            {
                AddErrorMessage(T("content.notFound"));
                return RedirectToAction(nameof(Index));
            }
            return View(content);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var content = await _contentService.GetByIdAsync(id);
            if (content != null)
            {
                // Delete physical file
                if (!string.IsNullOrEmpty(content.MediaPath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", content.MediaPath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                await _contentService.DeleteAsync(id);
                AddSuccessMessage(T("content.deletedSuccess"));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
