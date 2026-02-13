using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using DigitalSignage.Services;
using DigitalSignage.ViewModels;
using DigitalSignage.DTOs;
using DigitalSignage.Models.Common;
using AuthService = DigitalSignage.Services.IAuthorizationService;

namespace DigitalSignage.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly AuthService _authService;
        private readonly ICompanyService _companyService;
        private readonly IDepartmentService _departmentService;
        private readonly IMapper _mapper;

        public UserController(
            IUserService userService,
            AuthService authService,
            ICompanyService companyService,
            IDepartmentService departmentService,
            IMapper mapper)
        {
            _userService = userService;
            _authService = authService;
            _companyService = companyService;
            _departmentService = departmentService;
            _mapper = mapper;
        }

        // GET: User
        public async Task<IActionResult> Index(string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
        {
            try
            {
                const int pageSize = 10;

                // Tüm kullanıcıları al
                var allUsers = await _userService.GetAllAsync();
                IEnumerable<Models.User> query = allUsers;

                // Arama filtresi
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    query = query.Where(u =>
                        (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
                        (u.Email != null && u.Email.ToLower().Contains(search)) ||
                        (u.FirstName != null && u.FirstName.ToLower().Contains(search)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(search))
                    );
                }

                // Sıralama
                query = sortBy switch
                {
                    "UserName" => sortOrder == "asc"
                        ? query.OrderBy(u => u.UserName)
                        : query.OrderByDescending(u => u.UserName),
                    "Email" => sortOrder == "asc"
                        ? query.OrderBy(u => u.Email)
                        : query.OrderByDescending(u => u.Email),
                    "CreatedDate" => sortOrder == "asc"
                        ? query.OrderBy(u => u.CreatedDate)
                        : query.OrderByDescending(u => u.CreatedDate),
                    _ => query.OrderBy(u => u.UserName)
                };

                // Toplam sayı
                var totalCount = query.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Pagination
                var users = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModels = _mapper.Map<List<UserViewModel>>(users);

                // ViewBag
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.SearchTerm = search;
                ViewBag.SortBy = sortBy;
                ViewBag.SortOrder = sortOrder;

                return View(viewModels);
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorLoading"));
                return View(new List<UserViewModel>());
            }
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userService.GetUserWithRolesAsync(id);
                if (user == null)
                {
                    AddErrorMessage(T("common.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<UserViewModel>(user);
                return View(viewModel);
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorLoadingDetails"));
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: User/Create
        [Authorize(Roles = "SystemAdmin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Create(CreateUserDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(dto);

                var user = await _userService.CreateAsync(_mapper.Map<Models.User>(dto));

                AddSuccessMessage(T("user.createdSuccess"));
                return RedirectToAction(nameof(Details), new { id = user.UserID });
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorCreating"));
                return View(dto);
            }
        }

        // GET: User/Edit/5
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    AddErrorMessage(T("user.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                var dto = _mapper.Map<UpdateUserDTO>(user);
                return View(dto);
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorLoading"));
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Edit(int id, UpdateUserDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(dto);

                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    AddErrorMessage(T("user.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                _mapper.Map(dto, user);
                await _userService.UpdateAsync(user);

                AddSuccessMessage(T("user.updatedSuccess"));
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorUpdating"));
                return View(dto);
            }
        }

        // GET: User/Delete/5
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    AddErrorMessage(T("user.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<UserViewModel>(user);
                return View(viewModel);
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorLoading"));
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                AddSuccessMessage(T("user.deletedSuccess"));
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorDeleting"));
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int id, string currentPassword, string newPassword)
        {
            try
            {
                var success = await _userService.ChangePasswordAsync(id, currentPassword, newPassword);

                if (!success)
                {
                    AddErrorMessage(T("user.passwordIncorrect"));
                    return RedirectToAction(nameof(Details), new { id });
                }

                AddSuccessMessage(T("user.passwordChanged"));
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorChangingPassword"));
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // ============== ROLE MANAGEMENT ==============

        // GET: User/ManageRoles/5
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> ManageRoles(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Sadece SystemAdmin bu sayfaya erişebilir
                if (!await _authService.IsSystemAdminAsync(currentUserId))
                {
                    AddErrorMessage(T("error.unauthorized"));
                    return RedirectToAction(nameof(Index));
                }

                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    AddErrorMessage(T("user.notFound"));
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new UserRoleManagementViewModel
                {
                    User = user,
                    CompanyRoles = await GetUserCompanyRolesAsync(id),
                    DepartmentRoles = await GetUserDepartmentRolesAsync(id),
                    AvailableCompanies = await GetAvailableCompaniesAsync(currentUserId)
                };

                return View(viewModel);
            }
            catch (Exception)
            {
                AddErrorMessage(T("user.errorLoading"));
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: User/AssignCompanyRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> AssignCompanyRole([FromForm] AssignCompanyRoleDTO dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserName = User.Identity?.Name ?? "Unknown";

                // Yetki kontrolü: SystemAdmin veya o şirketin CompanyAdmin'i olmalı
                if (!await _authService.IsSystemAdminAsync(currentUserId) &&
                    !await _authService.IsCompanyAdminAsync(currentUserId, dto.CompanyID))
                {
                    AddErrorMessage(T("error.unauthorized"));
                    return RedirectToAction(nameof(ManageRoles), new { id = dto.UserID });
                }

                await _authService.AssignCompanyRoleAsync(
                    dto.UserID,
                    dto.CompanyID,
                    dto.Role,
                    currentUserName
                );

                AddSuccessMessage(T("role.companyRoleAssigned"));
                return RedirectToAction(nameof(ManageRoles), new { id = dto.UserID });
            }
            catch (Exception)
            {
                AddErrorMessage(T("role.errorAssigning"));
                return RedirectToAction(nameof(ManageRoles), new { id = dto.UserID });
            }
        }

        // POST: User/RemoveCompanyRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> RemoveCompanyRole(int userId, int companyId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Yetki kontrolü
                if (!await _authService.IsSystemAdminAsync(currentUserId) &&
                    !await _authService.IsCompanyAdminAsync(currentUserId, companyId))
                {
                    AddErrorMessage(T("error.unauthorized"));
                    return RedirectToAction(nameof(ManageRoles), new { id = userId });
                }

                await _authService.RemoveCompanyRoleAsync(userId, companyId);

                AddSuccessMessage(T("role.companyRoleRemoved"));
                return RedirectToAction(nameof(ManageRoles), new { id = userId });
            }
            catch (Exception)
            {
                AddErrorMessage(T("role.errorRemoving"));
                return RedirectToAction(nameof(ManageRoles), new { id = userId });
            }
        }

        // POST: User/AssignDepartmentRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> AssignDepartmentRole([FromForm] AssignDepartmentRoleDTO dto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserName = User.Identity?.Name ?? "Unknown";

                // Department'ın hangi şirkete ait olduğunu bul
                var department = await _departmentService.GetByIdAsync(dto.DepartmentID);
                if (department == null)
                {
                    AddErrorMessage(T("department.notFound"));
                    return RedirectToAction(nameof(ManageRoles), new { id = dto.UserID });
                }

                // Yetki kontrolü: SystemAdmin veya o şirketin CompanyAdmin'i olmalı
                if (!await _authService.IsSystemAdminAsync(currentUserId) &&
                    !await _authService.IsCompanyAdminAsync(currentUserId, department.CompanyID))
                {
                    AddErrorMessage(T("error.unauthorized"));
                    return RedirectToAction(nameof(ManageRoles), new { id = dto.UserID });
                }

                await _authService.AssignDepartmentRoleAsync(
                    dto.UserID,
                    dto.DepartmentID,
                    dto.Role,
                    currentUserName
                );

                AddSuccessMessage(T("role.departmentRoleAssigned"));
                return RedirectToAction(nameof(ManageRoles), new { id = dto.UserID });
            }
            catch (Exception)
            {
                AddErrorMessage(T("role.errorAssigning"));
                return RedirectToAction(nameof(ManageRoles), new { id = dto.UserID });
            }
        }

        // POST: User/RemoveDepartmentRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> RemoveDepartmentRole(int userId, int departmentId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                var department = await _departmentService.GetByIdAsync(departmentId);
                if (department == null)
                {
                    AddErrorMessage(T("department.notFound"));
                    return RedirectToAction(nameof(ManageRoles), new { id = userId });
                }

                // Yetki kontrolü
                if (!await _authService.IsSystemAdminAsync(currentUserId) &&
                    !await _authService.IsCompanyAdminAsync(currentUserId, department.CompanyID))
                {
                    AddErrorMessage(T("error.unauthorized"));
                    return RedirectToAction(nameof(ManageRoles), new { id = userId });
                }

                await _authService.RemoveDepartmentRoleAsync(userId, departmentId);

                AddSuccessMessage(T("role.departmentRoleRemoved"));
                return RedirectToAction(nameof(ManageRoles), new { id = userId });
            }
            catch (Exception)
            {
                AddErrorMessage(T("role.errorRemoving"));
                return RedirectToAction(nameof(ManageRoles), new { id = userId });
            }
        }

        // GET: User/GetCompanyDepartments - AJAX endpoint
        [HttpGet]
        public async Task<IActionResult> GetCompanyDepartments(int companyId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();

                // Yetki kontrolü
                if (!await _authService.CanAccessCompanyAsync(currentUserId, companyId))
                    return Forbid();

                var departments = await _departmentService.GetByCompanyIdAsync(companyId);

                return Json(departments.Select(d => new
                {
                    id = d.DepartmentID,
                    name = d.DepartmentName
                }));
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        // ============== HELPER METHODS ==============

        private async Task<List<UserCompanyRoleViewModel>> GetUserCompanyRolesAsync(int userId)
        {
            var companies = await _authService.GetUserCompaniesAsync(userId);
            var companyRoles = new List<UserCompanyRoleViewModel>();

            foreach (var company in companies)
            {
                var role = await _authService.GetCompanyRoleAsync(userId, company.CompanyID);
                if (role != null)
                {
                    companyRoles.Add(new UserCompanyRoleViewModel
                    {
                        UserCompanyRoleID = role.UserCompanyRoleID,
                        CompanyID = company.CompanyID,
                        CompanyName = company.CompanyName,
                        Role = role.Role,
                        IsActive = role.IsActive
                    });
                }
            }

            return companyRoles;
        }

        private async Task<List<UserDepartmentRoleViewModel>> GetUserDepartmentRolesAsync(int userId)
        {
            var companies = await _authService.GetUserCompaniesAsync(userId);
            var departmentRoles = new List<UserDepartmentRoleViewModel>();

            foreach (var company in companies)
            {
                var departments = await _authService.GetUserDepartmentsAsync(userId, company.CompanyID);

                foreach (var department in departments)
                {
                    var role = await _authService.GetDepartmentRoleAsync(userId, department.DepartmentID);
                    if (role != null)
                    {
                        departmentRoles.Add(new UserDepartmentRoleViewModel
                        {
                            UserDepartmentRoleID = role.UserDepartmentRoleID,
                            DepartmentID = department.DepartmentID,
                            DepartmentName = department.DepartmentName,
                            CompanyName = company.CompanyName,
                            Role = role.Role,
                            IsActive = role.IsActive
                        });
                    }
                }
            }

            return departmentRoles;
        }

        private async Task<List<Models.Company>> GetAvailableCompaniesAsync(int userId)
        {
            if (await _authService.IsSystemAdminAsync(userId))
            {
                var allCompanies = await _companyService.GetAllAsync();
                return allCompanies.Where(c => c.IsActive).ToList();
            }

            return await _authService.GetUserCompaniesAsync(userId);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var id) ? id : 0;
        }
    }
}
