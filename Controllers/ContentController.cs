using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;
using AuthService = DigitalSignage.Services.IAuthorizationService;

namespace DigitalSignage.Controllers
{
    public class ContentController : BaseController
    {
        private readonly IContentService _contentService;
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;
        private readonly string _uploadsPath;

        public ContentController(
            IContentService contentService,
            IDepartmentService departmentService,
            ICompanyService companyService,
            IConfiguration configuration,
            AuthService authService)
        {
            _contentService = contentService;
            _departmentService = departmentService;
            _companyService = companyService;
            _configuration = configuration;
            _authService = authService;
            _uploadsPath = _configuration["AppSettings:UploadsPath"] ?? "/uploads/";
        }

        public async Task<IActionResult> Index(int? departmentId, string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            const int pageSize = 10;
            var userId = GetCurrentUserId();
            if (!await _authService.HasAnyRoleAsync(userId))
                return AccessDenied();

            IEnumerable<Content> allContents;

            if (departmentId.HasValue)
            {
                if (!await _authService.CanAccessDepartmentAsync(userId, departmentId.Value))
                    return AccessDenied();

                allContents = await _contentService.GetByDepartmentIdAsync(departmentId.Value);
                ViewBag.DepartmentId = departmentId;

                // Departman bazlı düzenleme/silme izinleri
                ViewBag.CanEdit = await _authService.CanEditInDepartmentAsync(userId, departmentId.Value);
                ViewBag.CanDelete = await _authService.IsDepartmentManagerAsync(userId, departmentId.Value);
            }
            else
            {
                // Session'daki seçili şirkete göre filtrele
                var sessionCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
                var isSystemAdmin = await _authService.IsSystemAdminAsync(userId);

                if (isSystemAdmin && (!sessionCompanyId.HasValue || sessionCompanyId.Value <= 0))
                {
                    // SystemAdmin + şirket seçilmemiş → tümünü göster
                    allContents = await _contentService.GetAllAsync();
                }
                else
                {
                    // Seçili şirket veya kullanıcının erişebildiği şirketlerdeki içerikleri getir
                    List<Models.Company> companies;
                    if (sessionCompanyId.HasValue && sessionCompanyId.Value > 0)
                    {
                        // Seçili şirkete erişim kontrolü
                        if (!await _authService.CanAccessCompanyAsync(userId, sessionCompanyId.Value))
                            return AccessDenied();
                        var company = await _companyService.GetByIdAsync(sessionCompanyId.Value);
                        companies = company != null ? new List<Models.Company> { company } : new List<Models.Company>();
                    }
                    else
                    {
                        companies = await _authService.GetUserCompaniesAsync(userId);
                    }

                    var contentList = new List<Content>();
                    foreach (var company in companies)
                    {
                        var depts = await _authService.GetUserDepartmentsAsync(userId, company.CompanyID);
                        foreach (var dept in depts)
                        {
                            var deptContents = await _contentService.GetByDepartmentIdAsync(dept.DepartmentID);
                            contentList.AddRange(deptContents);
                        }
                    }
                    allContents = contentList;
                }

                ViewBag.CanEdit = isSystemAdmin || await _authService.HasAnyCompanyAdminRoleAsync(userId);
                ViewBag.CanDelete = ViewBag.CanEdit;
            }

            IEnumerable<Content> query = allContents;

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c =>
                    (c.ContentTitle != null && c.ContentTitle.ToLower().Contains(search)) ||
                    (c.ContentType != null && c.ContentType.ToLower().Contains(search)) ||
                    (c.MediaPath != null && c.MediaPath.ToLower().Contains(search))
                );
            }

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

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var contents = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(contents);
        }

        public async Task<IActionResult> Create(int? departmentId)
        {
            var userId = GetCurrentUserId();
            if (!await _authService.HasAnyRoleAsync(userId))
                return AccessDenied();

            if (departmentId.HasValue)
            {
                if (!await _authService.CanEditInDepartmentAsync(userId, departmentId.Value))
                    return AccessDenied();

                ViewBag.DepartmentId = departmentId;
            }
            else
            {
                ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Content content, IFormFile? mediaFile)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanEditInDepartmentAsync(userId, content.DepartmentID))
                return AccessDenied();

            // Form'da gelmeyen non-nullable property'leri ModelState'ten çıkar
            ModelState.Remove("Department");
            ModelState.Remove("PageContents");
            ModelState.Remove("CreatedBy");

            if (ModelState.IsValid)
            {
                if (mediaFile != null && mediaFile.Length > 0)
                {
                    // Image, Video ve PDF dosya yükleme
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".mp4", ".webm", ".ogg", ".pdf" };
                    var ext = Path.GetExtension(mediaFile.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        AddErrorMessage(T("content.unsupportedFormat"));
                        ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
                        return View(content);
                    }

                    var fileName = Guid.NewGuid().ToString() + ext;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await mediaFile.CopyToAsync(stream);
                    }

                    content.MediaPath = _uploadsPath + fileName;
                }

                // URL tipi için ContentData'ya URL'yi kaydet
                if (content.ContentType == "URL" && string.IsNullOrEmpty(content.ContentData))
                {
                    AddErrorMessage(T("content.urlRequired"));
                    ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
                    return View(content);
                }

                await _contentService.CreateAsync(content);
                AddSuccessMessage(T("content.createdSuccess"));
                return RedirectToAction(nameof(Index), new { departmentId = content.DepartmentID });
            }

            ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
            return View(content);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var content = await _contentService.GetContentWithDepartmentAsync(id);

            if (content == null)
            {
                AddErrorMessage(T("content.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.CanAccessDepartmentAsync(userId, content.DepartmentID))
                return AccessDenied();

            return View(content);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();
            var content = await _contentService.GetByIdAsync(id);

            if (content == null)
            {
                AddErrorMessage(T("content.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.CanEditInDepartmentAsync(userId, content.DepartmentID))
                return AccessDenied();

            ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Content content, IFormFile? file)
        {
            var userId = GetCurrentUserId();

            if (id != content.ContentID)
            {
                AddErrorMessage(T("content.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.CanEditInDepartmentAsync(userId, content.DepartmentID))
                return AccessDenied();

            // Form'da gelmeyen non-nullable property'leri ModelState'ten çıkar
            ModelState.Remove("Department");
            ModelState.Remove("PageContents");
            ModelState.Remove("CreatedBy");

            if (ModelState.IsValid)
            {
                if (file != null && file.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".mp4", ".webm", ".ogg", ".pdf" };
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        AddErrorMessage(T("content.unsupportedFormat"));
                        ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
                        return View(content);
                    }

                    var fileName = Guid.NewGuid().ToString() + ext;
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    if (!string.IsNullOrEmpty(content.MediaPath))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", content.MediaPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    content.MediaPath = _uploadsPath + fileName;
                }

                await _contentService.UpdateAsync(content);
                AddSuccessMessage(T("content.updatedSuccess"));
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = await GetAccessibleDepartmentsAsync(userId);
            return View(content);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            var content = await _contentService.GetByIdAsync(id);

            if (content == null)
            {
                AddErrorMessage(T("content.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.IsDepartmentManagerAsync(userId, content.DepartmentID))
                return AccessDenied();

            return View(content);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();
            var content = await _contentService.GetByIdAsync(id);

            if (content == null)
            {
                AddErrorMessage(T("content.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.IsDepartmentManagerAsync(userId, content.DepartmentID))
                return AccessDenied();

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
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Department>> GetAccessibleDepartmentsAsync(int userId)
        {
            var sessionCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
            var departments = new List<Department>();

            if (sessionCompanyId.HasValue && sessionCompanyId.Value > 0)
            {
                var depts = await _authService.GetUserDepartmentsAsync(userId, sessionCompanyId.Value);
                departments.AddRange(depts);
            }
            else
            {
                var userCompanies = await _authService.GetUserCompaniesAsync(userId);
                foreach (var company in userCompanies)
                {
                    var depts = await _authService.GetUserDepartmentsAsync(userId, company.CompanyID);
                    departments.AddRange(depts);
                }
            }
            return departments;
        }
    }
}
