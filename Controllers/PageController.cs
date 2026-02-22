using DigitalSignage.Models;
using DigitalSignage.Services;
using Microsoft.AspNetCore.Mvc;
using AuthService = DigitalSignage.Services.IAuthorizationService;

namespace DigitalSignage.Controllers
{
    public class PageController : BaseController
    {
        private readonly IPageService _pageService;
        private readonly ILayoutService _layoutService;
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;
        private readonly IContentService _contentService;
        private readonly AuthService _authService;

        public PageController(
            IPageService pageService,
            ILayoutService layoutService,
            IDepartmentService departmentService,
            ICompanyService companyService,
            IContentService contentService,
            AuthService authService)
        {
            _pageService = pageService;
            _layoutService = layoutService;
            _departmentService = departmentService;
            _companyService = companyService;
            _contentService = contentService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(int? departmentId, string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            const int pageSize = 10;
            var userId = GetCurrentUserId();
            if (!await _authService.HasAnyRoleAsync(userId))
                return AccessDenied();

            IEnumerable<Page> allPages;

            if (departmentId.HasValue)
            {
                if (!await _authService.CanAccessDepartmentAsync(userId, departmentId.Value))
                    return AccessDenied();

                allPages = await _pageService.GetByDepartmentIdAsync(departmentId.Value);
                ViewBag.DepartmentId = departmentId;

                ViewBag.CanEdit = await _authService.CanEditInDepartmentAsync(userId, departmentId.Value);
                ViewBag.CanDelete = await _authService.IsDepartmentManagerAsync(userId, departmentId.Value);
            }
            else
            {
                var sessionCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
                var isSystemAdmin = await _authService.IsSystemAdminAsync(userId);

                if (isSystemAdmin && (!sessionCompanyId.HasValue || sessionCompanyId.Value <= 0))
                {
                    allPages = await _pageService.GetAllAsync();
                }
                else
                {
                    List<Models.Company> companies;
                    if (sessionCompanyId.HasValue && sessionCompanyId.Value > 0)
                    {
                        if (!await _authService.CanAccessCompanyAsync(userId, sessionCompanyId.Value))
                            return AccessDenied();
                        var company = await _companyService.GetByIdAsync(sessionCompanyId.Value);
                        companies = company != null ? new List<Models.Company> { company } : new List<Models.Company>();
                    }
                    else
                    {
                        companies = await _authService.GetUserCompaniesAsync(userId);
                    }

                    var pageList = new List<Page>();
                    foreach (var company in companies)
                    {
                        var depts = await _authService.GetUserDepartmentsAsync(userId, company.CompanyID);
                        foreach (var dept in depts)
                        {
                            var deptPages = await _pageService.GetByDepartmentIdAsync(dept.DepartmentID);
                            pageList.AddRange(deptPages);
                        }
                    }
                    allPages = pageList;
                }

                // Kullanıcının herhangi bir departmanda edit yetkisi var mı kontrol et
                var canEdit = isSystemAdmin || await _authService.HasAnyCompanyAdminRoleAsync(userId);
                if (!canEdit)
                {
                    // DepartmentManager/Editor rolü olan kullanıcılar da sayfa oluşturabilir
                    var userCompanies = await _authService.GetUserCompaniesAsync(userId);
                    foreach (var company in userCompanies)
                    {
                        var depts = await _authService.GetUserDepartmentsAsync(userId, company.CompanyID);
                        foreach (var dept in depts)
                        {
                            if (await _authService.CanEditInDepartmentAsync(userId, dept.DepartmentID))
                            {
                                canEdit = true;
                                break;
                            }
                        }
                        if (canEdit) break;
                    }
                }
                ViewBag.CanEdit = canEdit;
                ViewBag.CanDelete = isSystemAdmin || await _authService.HasAnyCompanyAdminRoleAsync(userId);
            }

            IEnumerable<Page> query = allPages;

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(p =>
                    (p.PageTitle != null && p.PageTitle.ToLower().Contains(search)) ||
                    (p.Description != null && p.Description.ToLower().Contains(search)) ||
                    (p.PageName != null && p.PageName.ToLower().Contains(search))
                );
            }

            query = sortBy switch
            {
                "PageTitle" => sortOrder == "asc"
                    ? query.OrderBy(p => p.PageTitle)
                    : query.OrderByDescending(p => p.PageTitle),
                "CreatedDate" => sortOrder == "asc"
                    ? query.OrderBy(p => p.CreatedDate)
                    : query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderBy(p => p.PageTitle)
            };

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var pages = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = search;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(pages);
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
                await LoadAccessibleDepartmentsAsync(userId);

                // Erişilebilir departman yoksa yetki yok demektir
                var departments = ViewBag.Departments as List<Department>;
                if (departments == null || !departments.Any())
                    return AccessDenied();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Page page)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanEditInDepartmentAsync(userId, page.DepartmentID))
                return AccessDenied();

            // PageCode ve LayoutID otomatik atanacak, navigation property'ler bind edilmiyor
            ModelState.Remove("PageCode");
            ModelState.Remove("LayoutID");
            ModelState.Remove("Department");
            ModelState.Remove("Layout");
            ModelState.Remove("PageContents");
            ModelState.Remove("PageSections");
            ModelState.Remove("SchedulePages");

            if (ModelState.IsValid)
            {
                page.LayoutID = null;
                await _pageService.CreateAsync(page);
                AddSuccessMessage(T("page.createdSuccess"));
                return RedirectToAction(nameof(Design), new { id = page.PageID });
            }

            // Validation fail — erişilebilir veriyi tekrar yükle
            await LoadAccessibleDepartmentsAsync(userId, page.DepartmentID);
            return View(page);
        }

        public async Task<IActionResult> Design(int id)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanAccessPageAsync(userId, id))
                return AccessDenied();

            var page = await _pageService.GetPageFullDetailsAsync(id);
            if (page == null)
            {
                AddErrorMessage(T("page.notFound"));
                return RedirectToAction(nameof(Index));
            }

            // Layout'ları departmanın şirketine göre yükle
            var dept = await _departmentService.GetByIdAsync(page.DepartmentID);
            if (dept != null)
            {
                ViewBag.Layouts = await _layoutService.GetByCompanyIdAsync(dept.CompanyID);
            }
            else
            {
                ViewBag.Layouts = new List<Layout>();
            }

            ViewBag.CanEdit = await _authService.CanEditPageAsync(userId, id);

            // Section'lara atanmış içeriklerin map'i
            ViewBag.SectionContentMap = await _pageService.GetSectionContentMapAsync(id);

            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignLayout(int pageId, int layoutId)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanEditPageAsync(userId, pageId))
                return AccessDenied();

            var result = await _pageService.AssignLayoutAsync(pageId, layoutId);
            if (result)
            {
                AddSuccessMessage(T("page.layoutAssigned"));
            }
            else
            {
                AddErrorMessage(T("page.notFound"));
            }

            return RedirectToAction(nameof(Design), new { id = pageId });
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanAccessPageAsync(userId, id))
                return AccessDenied();

            var page = await _pageService.GetPageFullDetailsAsync(id);
            if (page == null)
            {
                AddErrorMessage(T("page.notFound"));
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CanEdit = await _authService.CanEditPageAsync(userId, id);
            ViewBag.SectionContentMap = await _pageService.GetSectionContentMapAsync(id);

            return View(page);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanEditPageAsync(userId, id))
                return AccessDenied();

            var page = await _pageService.GetByIdAsync(id);
            if (page == null)
            {
                AddErrorMessage(T("page.notFound"));
                return RedirectToAction(nameof(Index));
            }

            await LoadAccessibleDepartmentsAsync(userId, page.DepartmentID);
            return View(page);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Page page)
        {
            var userId = GetCurrentUserId();

            if (id != page.PageID)
            {
                AddErrorMessage(T("page.notFound"));
                return RedirectToAction(nameof(Index));
            }

            if (!await _authService.CanEditPageAsync(userId, id))
                return AccessDenied();

            // PageCode readonly, LayoutID Design'dan değiştirilecek, navigation property'ler bind edilmiyor
            ModelState.Remove("PageCode");
            ModelState.Remove("LayoutID");
            ModelState.Remove("Department");
            ModelState.Remove("Layout");
            ModelState.Remove("PageContents");
            ModelState.Remove("PageSections");
            ModelState.Remove("SchedulePages");

            if (ModelState.IsValid)
            {
                // Tracked entity üzerinden güncelle (aynı PK ile iki farklı instance track edilemez)
                var existingPage = await _pageService.GetByIdAsync(id);
                if (existingPage == null)
                {
                    AddErrorMessage(T("page.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                existingPage.PageName = page.PageName;
                existingPage.PageTitle = page.PageTitle;
                existingPage.Description = page.Description;
                existingPage.DepartmentID = page.DepartmentID;
                existingPage.IsActive = page.IsActive;

                await _pageService.UpdateAsync(existingPage);
                AddSuccessMessage(T("page.updatedSuccess"));
                return RedirectToAction(nameof(Index), new { departmentId = existingPage.DepartmentID });
            }

            await LoadAccessibleDepartmentsAsync(userId, page.DepartmentID);
            return View(page);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanModifyPageAsync(userId, id))
                return AccessDenied();

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
            var userId = GetCurrentUserId();

            if (!await _authService.CanModifyPageAsync(userId, id))
                return AccessDenied();

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

        [HttpGet]
        public async Task<IActionResult> GetDepartmentContents(int departmentId)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanAccessDepartmentAsync(userId, departmentId))
                return Json(new { success = false, message = "Erişim reddedildi." });

            var contents = await _contentService.GetActiveContentsByDepartmentAsync(departmentId);
            var result = contents.Select(c => new
            {
                contentId = c.ContentID,
                contentTitle = c.ContentTitle,
                contentType = c.ContentType,
                thumbnailPath = c.ThumbnailPath
            });

            return Json(new { success = true, contents = result });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignContentToSection(int pageId, string sectionPosition, int contentId)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanEditPageAsync(userId, pageId))
                return Json(new { success = false, message = "Erişim reddedildi." });

            var result = await _pageService.AssignContentToSectionAsync(pageId, sectionPosition, contentId);
            if (!result)
                return Json(new { success = false, message = T("page.notFound") });

            // Atanan içeriğin bilgilerini geri dön
            var content = await _contentService.GetByIdAsync(contentId);
            return Json(new
            {
                success = true,
                message = T("page.contentAssigned"),
                content = new
                {
                    contentTitle = content?.ContentTitle,
                    contentType = content?.ContentType
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveContentFromSection(int pageId, string sectionPosition)
        {
            var userId = GetCurrentUserId();

            if (!await _authService.CanEditPageAsync(userId, pageId))
                return Json(new { success = false, message = "Erişim reddedildi." });

            var result = await _pageService.RemoveContentFromSectionAsync(pageId, sectionPosition);
            if (!result)
                return Json(new { success = false, message = T("page.notFound") });

            return Json(new { success = true, message = T("page.contentRemoved") });
        }

        private async Task LoadAccessibleDepartmentsAsync(int userId, int? currentDepartmentId = null)
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
            ViewBag.Departments = departments;

            if (currentDepartmentId.HasValue)
            {
                ViewBag.DepartmentId = currentDepartmentId;
            }
        }
    }
}
