# Authorization & Role Management System

## Genel Bakış

Digital Signage platformu, **çok seviyeli yetkilendirme sistemi** kullanır. Kullanıcılar, sistem seviyesinde, şirket seviyesinde veya departman seviyesinde farklı rollere sahip olabilir. Her rol, kullanıcının erişebildiği kaynakları ve yapabildiği işlemleri belirler.

---

## Role Hierarchy (Rol Hiyerarşisi)

### Seviyeler

```
┌─────────────────────────────────────────┐
│         SystemAdmin (En Üst)           │
│  - Tüm şirketlere tam erişim            │
│  - Platform yönetimi                    │
│  - Tüm kullanıcıları yönetme            │
└─────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│         CompanyAdmin (Şirket)           │
│  - Bir veya birden fazla şirket admini  │
│  - O şirketin TÜM departmanları         │
│  - Şirket konfigürasyonu                │
│  - Kullanıcı rol ataması                │
└─────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│     DepartmentManager (Departman)       │
│  - Belirli departman(lar) yönetimi      │
│  - Sayfa oluşturma/düzenleme            │
│  - Layout & Content yönetimi            │
│  - Schedule yönetimi                    │
└─────────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│           Viewer (Okuma)                │
│  - Sadece görüntüleme                   │
│  - Hiçbir değişiklik yapamaz            │
└─────────────────────────────────────────┘
```

### Yetki Matrisi

| Role | SystemAdmin | CompanyAdmin | DepartmentManager | Viewer |
|------|-------------|--------------|-------------------|--------|
| **Tüm Şirketler** | ✅ Tam Erişim | ❌ | ❌ | ❌ |
| **Şirket Yönetimi** | ✅ | ✅ (Atanan şirketler) | ❌ | ❌ |
| **Tüm Departmanlar** | ✅ | ✅ (Admin olduğu şirkette) | ❌ | ❌ |
| **Belirli Departmanlar** | ✅ | ✅ | ✅ (Atanan departmanlar) | ❌ |
| **Sayfa/Layout CRUD** | ✅ | ✅ | ✅ (Kendi departmanında) | ❌ |
| **Content Yönetimi** | ✅ | ✅ | ✅ (Kendi departmanında) | ❌ |
| **Kullanıcı Rol Ataması** | ✅ | ✅ (Kendi şirketinde) | ❌ | ❌ |
| **Görüntüleme** | ✅ | ✅ | ✅ | ✅ |

---

## Database Schema

### User Entity

```csharp
public class User
{
    public int UserID { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public bool IsSystemAdmin { get; set; }  // Platform admini
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation Properties
    public ICollection<UserCompanyRole> UserCompanyRoles { get; set; }
    public ICollection<UserDepartmentRole> UserDepartmentRoles { get; set; }
}
```

### UserCompanyRole Entity (Şirket Seviyesi)

```csharp
public class UserCompanyRole
{
    public int UserCompanyRoleID { get; set; }
    public int UserID { get; set; }
    public int CompanyID { get; set; }

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Viewer";
    // Possible values: "CompanyAdmin", "Viewer"

    public bool IsActive { get; set; } = true;
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string AssignedBy { get; set; } = "System";

    // Navigation Properties
    public User User { get; set; } = null!;
    public Company Company { get; set; } = null!;
}
```

### UserDepartmentRole Entity (Departman Seviyesi) 🆕

```csharp
public class UserDepartmentRole
{
    public int UserDepartmentRoleID { get; set; }
    public int UserID { get; set; }
    public int DepartmentID { get; set; }

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Viewer";
    // Possible values: "DepartmentManager", "Editor", "Viewer"

    public bool IsActive { get; set; } = true;
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string AssignedBy { get; set; } = "System";

    // Navigation Properties
    public User User { get; set; } = null!;
    public Department Department { get; set; } = null!;
}
```

### Database Relationships

```
User (1) ────── (N) UserCompanyRole (N) ────── (1) Company
  │
  │
  └─────────── (N) UserDepartmentRole (N) ────── (1) Department
                                                        │
                                                        └─── (N) Page
                                                               └─── (1) Layout
                                                                      └─── (N) Section
```

---

## Authorization Logic

### İşleyiş Mantığı

#### 1. SystemAdmin Check
```csharp
if (user.IsSystemAdmin)
{
    // Her şeye erişim var
    return true;
}
```

#### 2. Company Access Check
```csharp
// Kullanıcı bu şirkete erişebilir mi?
var companyRole = await _userCompanyRoleRepository.FirstOrDefaultAsync(
    ucr => ucr.UserID == userId &&
           ucr.CompanyID == companyId &&
           ucr.IsActive
);

if (companyRole == null)
    return false; // Erişim yok

// CompanyAdmin ise TÜM departmanlara erişim var
if (companyRole.Role == "CompanyAdmin")
    return true;
```

#### 3. Department Access Check
```csharp
// Kullanıcı bu departmana erişebilir mi?

// Önce Company Admin mi kontrol et
var companyRole = await GetCompanyRoleAsync(userId, department.CompanyID);
if (companyRole?.Role == "CompanyAdmin")
    return true; // Company Admin her departmana erişir

// Department seviyesinde rol var mı?
var departmentRole = await _userDepartmentRoleRepository.FirstOrDefaultAsync(
    udr => udr.UserID == userId &&
           udr.DepartmentID == departmentId &&
           udr.IsActive
);

return departmentRole != null;
```

#### 4. Permission Check Flowchart

```
                    ┌─────────────────┐
                    │  Access Request │
                    └────────┬────────┘
                             │
                             ▼
                    ┌─────────────────┐
                    │ IsSystemAdmin?  │
                    └────────┬────────┘
                        Yes  │  No
                    ┌────────┴────────┐
                    ▼                 ▼
              ┌─────────┐    ┌────────────────┐
              │ ALLOW   │    │ Has CompanyRole?│
              └─────────┘    └────────┬────────┘
                                 Yes  │  No
                            ┌─────────┴─────────┐
                            ▼                   ▼
                   ┌─────────────────┐    ┌─────────┐
                   │ CompanyAdmin?   │    │  DENY   │
                   └────────┬────────┘    └─────────┘
                       Yes  │  No
                   ┌────────┴────────┐
                   ▼                 ▼
             ┌─────────┐    ┌──────────────────┐
             │ ALLOW   │    │Has DepartmentRole?│
             │(Full Co)│    └────────┬──────────┘
             └─────────┘         Yes │  No
                            ┌────────┴────────┐
                            ▼                 ▼
                      ┌─────────┐       ┌─────────┐
                      │ ALLOW   │       │  DENY   │
                      │(Dept.)  │       └─────────┘
                      └─────────┘
```

---

## IAuthorizationService Interface

### Interface Definition

```csharp
namespace DigitalSignage.Services
{
    public interface IAuthorizationService
    {
        // System Level
        Task<bool> IsSystemAdminAsync(int userId);

        // Company Level
        Task<bool> CanAccessCompanyAsync(int userId, int companyId);
        Task<bool> IsCompanyAdminAsync(int userId, int companyId);
        Task<List<Company>> GetUserCompaniesAsync(int userId);
        Task<UserCompanyRole?> GetCompanyRoleAsync(int userId, int companyId);

        // Department Level
        Task<bool> CanAccessDepartmentAsync(int userId, int departmentId);
        Task<bool> IsDepartmentManagerAsync(int userId, int departmentId);
        Task<List<Department>> GetUserDepartmentsAsync(int userId, int companyId);
        Task<UserDepartmentRole?> GetDepartmentRoleAsync(int userId, int departmentId);

        // Page Level (cascades from Department)
        Task<bool> CanAccessPageAsync(int userId, int pageId);
        Task<bool> CanModifyPageAsync(int userId, int pageId);

        // Role Assignment (CompanyAdmin only)
        Task AssignCompanyRoleAsync(int userId, int companyId, string role, string assignedBy);
        Task RemoveCompanyRoleAsync(int userId, int companyId);
        Task AssignDepartmentRoleAsync(int userId, int departmentId, string role, string assignedBy);
        Task RemoveDepartmentRoleAsync(int userId, int departmentId);
    }
}
```

### Service Implementation

```csharp
public class AuthorizationService : IAuthorizationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<AuthorizationService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    // ============== SYSTEM LEVEL ==============

    public async Task<bool> IsSystemAdminAsync(int userId)
    {
        var cacheKey = $"user_sysadmin_{userId}";

        if (_cache.TryGetValue(cacheKey, out bool isAdmin))
            return isAdmin;

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        isAdmin = user?.IsSystemAdmin ?? false;

        _cache.Set(cacheKey, isAdmin, TimeSpan.FromMinutes(15));
        return isAdmin;
    }

    // ============== COMPANY LEVEL ==============

    public async Task<bool> CanAccessCompanyAsync(int userId, int companyId)
    {
        // System Admin her şeye erişir
        if (await IsSystemAdminAsync(userId))
            return true;

        var cacheKey = $"user_{userId}_company_{companyId}_access";

        if (_cache.TryGetValue(cacheKey, out bool hasAccess))
            return hasAccess;

        var role = await _unitOfWork.UserCompanyRoles.FirstOrDefaultAsync(
            ucr => ucr.UserID == userId &&
                   ucr.CompanyID == companyId &&
                   ucr.IsActive
        );

        hasAccess = role != null;
        _cache.Set(cacheKey, hasAccess, TimeSpan.FromMinutes(10));

        return hasAccess;
    }

    public async Task<bool> IsCompanyAdminAsync(int userId, int companyId)
    {
        if (await IsSystemAdminAsync(userId))
            return true;

        var role = await GetCompanyRoleAsync(userId, companyId);
        return role?.Role == "CompanyAdmin";
    }

    public async Task<UserCompanyRole?> GetCompanyRoleAsync(int userId, int companyId)
    {
        return await _unitOfWork.UserCompanyRoles.FirstOrDefaultAsync(
            ucr => ucr.UserID == userId &&
                   ucr.CompanyID == companyId &&
                   ucr.IsActive
        );
    }

    public async Task<List<Company>> GetUserCompaniesAsync(int userId)
    {
        if (await IsSystemAdminAsync(userId))
        {
            // System Admin tüm şirketleri görür
            return await _unitOfWork.Companies.GetAllAsync();
        }

        var companyRoles = await _unitOfWork.UserCompanyRoles.FindAsync(
            ucr => ucr.UserID == userId && ucr.IsActive
        );

        var companyIds = companyRoles.Select(ucr => ucr.CompanyID).Distinct().ToList();
        var companies = new List<Company>();

        foreach (var companyId in companyIds)
        {
            var company = await _unitOfWork.Companies.GetByIdAsync(companyId);
            if (company != null && company.IsActive)
                companies.Add(company);
        }

        return companies;
    }

    // ============== DEPARTMENT LEVEL ==============

    public async Task<bool> CanAccessDepartmentAsync(int userId, int departmentId)
    {
        // System Admin her şeye erişir
        if (await IsSystemAdminAsync(userId))
            return true;

        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
        if (department == null)
            return false;

        // Company Admin o şirketin tüm departmanlarına erişir
        if (await IsCompanyAdminAsync(userId, department.CompanyID))
            return true;

        // Department seviyesinde rol kontrolü
        var departmentRole = await _unitOfWork.UserDepartmentRoles.FirstOrDefaultAsync(
            udr => udr.UserID == userId &&
                   udr.DepartmentID == departmentId &&
                   udr.IsActive
        );

        return departmentRole != null;
    }

    public async Task<bool> IsDepartmentManagerAsync(int userId, int departmentId)
    {
        if (await IsSystemAdminAsync(userId))
            return true;

        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
        if (department == null)
            return false;

        // Company Admin = Department Manager yetkisine sahip
        if (await IsCompanyAdminAsync(userId, department.CompanyID))
            return true;

        var role = await GetDepartmentRoleAsync(userId, departmentId);
        return role?.Role == "DepartmentManager";
    }

    public async Task<UserDepartmentRole?> GetDepartmentRoleAsync(int userId, int departmentId)
    {
        return await _unitOfWork.UserDepartmentRoles.FirstOrDefaultAsync(
            udr => udr.UserID == userId &&
                   udr.DepartmentID == departmentId &&
                   udr.IsActive
        );
    }

    public async Task<List<Department>> GetUserDepartmentsAsync(int userId, int companyId)
    {
        // Company Admin tüm departmanları görür
        if (await IsCompanyAdminAsync(userId, companyId))
        {
            return await _unitOfWork.Departments.FindAsync(
                d => d.CompanyID == companyId && d.IsActive
            );
        }

        // Kullanıcının atandığı departmanları al
        var departmentRoles = await _unitOfWork.UserDepartmentRoles.FindAsync(
            udr => udr.UserID == userId && udr.IsActive
        );

        var departments = new List<Department>();
        foreach (var role in departmentRoles)
        {
            var dept = await _unitOfWork.Departments.GetByIdAsync(role.DepartmentID);
            if (dept != null && dept.CompanyID == companyId && dept.IsActive)
                departments.Add(dept);
        }

        return departments;
    }

    // ============== PAGE LEVEL ==============

    public async Task<bool> CanAccessPageAsync(int userId, int pageId)
    {
        var page = await _unitOfWork.Pages.GetByIdAsync(pageId);
        if (page == null)
            return false;

        return await CanAccessDepartmentAsync(userId, page.DepartmentID);
    }

    public async Task<bool> CanModifyPageAsync(int userId, int pageId)
    {
        var page = await _unitOfWork.Pages.GetByIdAsync(pageId);
        if (page == null)
            return false;

        return await IsDepartmentManagerAsync(userId, page.DepartmentID);
    }

    // ============== ROLE ASSIGNMENT ==============

    public async Task AssignCompanyRoleAsync(int userId, int companyId, string role, string assignedBy)
    {
        // Mevcut rol var mı kontrol et
        var existingRole = await GetCompanyRoleAsync(userId, companyId);

        if (existingRole != null)
        {
            // Role güncelle
            existingRole.Role = role;
            existingRole.AssignedDate = DateTime.UtcNow;
            existingRole.AssignedBy = assignedBy;
            await _unitOfWork.UserCompanyRoles.UpdateAsync(existingRole);
        }
        else
        {
            // Yeni rol ekle
            var newRole = new UserCompanyRole
            {
                UserID = userId,
                CompanyID = companyId,
                Role = role,
                IsActive = true,
                AssignedDate = DateTime.UtcNow,
                AssignedBy = assignedBy
            };
            await _unitOfWork.UserCompanyRoles.AddAsync(newRole);
        }

        await _unitOfWork.CommitAsync();

        // Cache temizle
        ClearUserCache(userId);

        _logger.LogInformation($"User {userId} assigned role '{role}' for company {companyId} by {assignedBy}");
    }

    public async Task RemoveCompanyRoleAsync(int userId, int companyId)
    {
        var role = await GetCompanyRoleAsync(userId, companyId);
        if (role != null)
        {
            await _unitOfWork.UserCompanyRoles.DeleteAsync(role.UserCompanyRoleID);
            await _unitOfWork.CommitAsync();

            ClearUserCache(userId);

            _logger.LogInformation($"User {userId} removed from company {companyId}");
        }
    }

    public async Task AssignDepartmentRoleAsync(int userId, int departmentId, string role, string assignedBy)
    {
        var existingRole = await GetDepartmentRoleAsync(userId, departmentId);

        if (existingRole != null)
        {
            existingRole.Role = role;
            existingRole.AssignedDate = DateTime.UtcNow;
            existingRole.AssignedBy = assignedBy;
            await _unitOfWork.UserDepartmentRoles.UpdateAsync(existingRole);
        }
        else
        {
            var newRole = new UserDepartmentRole
            {
                UserID = userId,
                DepartmentID = departmentId,
                Role = role,
                IsActive = true,
                AssignedDate = DateTime.UtcNow,
                AssignedBy = assignedBy
            };
            await _unitOfWork.UserDepartmentRoles.AddAsync(newRole);
        }

        await _unitOfWork.CommitAsync();

        ClearUserCache(userId);

        _logger.LogInformation($"User {userId} assigned role '{role}' for department {departmentId} by {assignedBy}");
    }

    public async Task RemoveDepartmentRoleAsync(int userId, int departmentId)
    {
        var role = await GetDepartmentRoleAsync(userId, departmentId);
        if (role != null)
        {
            await _unitOfWork.UserDepartmentRoles.DeleteAsync(role.UserDepartmentRoleID);
            await _unitOfWork.CommitAsync();

            ClearUserCache(userId);

            _logger.LogInformation($"User {userId} removed from department {departmentId}");
        }
    }

    // ============== CACHE MANAGEMENT ==============

    private void ClearUserCache(int userId)
    {
        // Kullanıcı ile ilgili tüm cache'leri temizle
        _cache.Remove($"user_sysadmin_{userId}");

        // Company ve department cache'lerini temizlemek için pattern kullan
        // Not: Production'da distributed cache kullanılıyorsa farklı bir yaklaşım gerekir
    }
}
```

---

## Controller Implementation

### UserController - Role Management

```csharp
[Authorize]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    private readonly IAuthorizationService _authService;
    private readonly ICompanyService _companyService;
    private readonly IDepartmentService _departmentService;

    /// <summary>
    /// Kullanıcı rol yönetim sayfası
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ManageRoles(int id)
    {
        var currentUserId = GetCurrentUserId();

        // Sadece SystemAdmin veya CompanyAdmin bu sayfaya erişebilir
        if (!await _authService.IsSystemAdminAsync(currentUserId))
        {
            AddErrorMessage(T("error.unauthorized"));
            return RedirectToAction("Index");
        }

        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            AddErrorMessage(T("user.notFound"));
            return RedirectToAction("Index");
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

    /// <summary>
    /// Company role ata
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignCompanyRole([FromBody] AssignCompanyRoleDTO dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = User.Identity?.Name ?? "Unknown";

            // Yetki kontrolü: SystemAdmin veya o şirketin CompanyAdmin'i olmalı
            if (!await _authService.IsSystemAdminAsync(currentUserId) &&
                !await _authService.IsCompanyAdminAsync(currentUserId, dto.CompanyID))
            {
                return Forbid();
            }

            await _authService.AssignCompanyRoleAsync(
                dto.UserID,
                dto.CompanyID,
                dto.Role,
                currentUserName
            );

            AddSuccessMessage(T("role.companyRoleAssigned"));
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning company role");
            return BadRequest(T("role.errorAssigning"));
        }
    }

    /// <summary>
    /// Company role kaldır
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveCompanyRole(int userId, int companyId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            // Yetki kontrolü
            if (!await _authService.IsSystemAdminAsync(currentUserId) &&
                !await _authService.IsCompanyAdminAsync(currentUserId, companyId))
            {
                return Forbid();
            }

            await _authService.RemoveCompanyRoleAsync(userId, companyId);

            AddSuccessMessage(T("role.companyRoleRemoved"));
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing company role");
            return BadRequest(T("role.errorRemoving"));
        }
    }

    /// <summary>
    /// Department role ata
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignDepartmentRole([FromBody] AssignDepartmentRoleDTO dto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = User.Identity?.Name ?? "Unknown";

            // Department'ın hangi şirkete ait olduğunu bul
            var department = await _departmentService.GetByIdAsync(dto.DepartmentID);
            if (department == null)
                return NotFound();

            // Yetki kontrolü: SystemAdmin veya o şirketin CompanyAdmin'i olmalı
            if (!await _authService.IsSystemAdminAsync(currentUserId) &&
                !await _authService.IsCompanyAdminAsync(currentUserId, department.CompanyID))
            {
                return Forbid();
            }

            await _authService.AssignDepartmentRoleAsync(
                dto.UserID,
                dto.DepartmentID,
                dto.Role,
                currentUserName
            );

            AddSuccessMessage(T("role.departmentRoleAssigned"));
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning department role");
            return BadRequest(T("role.errorAssigning"));
        }
    }

    /// <summary>
    /// Department role kaldır
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveDepartmentRole(int userId, int departmentId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();

            var department = await _departmentService.GetByIdAsync(departmentId);
            if (department == null)
                return NotFound();

            // Yetki kontrolü
            if (!await _authService.IsSystemAdminAsync(currentUserId) &&
                !await _authService.IsCompanyAdminAsync(currentUserId, department.CompanyID))
            {
                return Forbid();
            }

            await _authService.RemoveDepartmentRoleAsync(userId, departmentId);

            AddSuccessMessage(T("role.departmentRoleRemoved"));
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing department role");
            return BadRequest(T("role.errorRemoving"));
        }
    }

    /// <summary>
    /// AJAX: Şirkete ait departmanları getir
    /// </summary>
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company departments");
            return BadRequest();
        }
    }
}
```

### DepartmentController - Authorization Check

```csharp
[Authorize]
public class DepartmentController : BaseController
{
    private readonly IDepartmentService _departmentService;
    private readonly IAuthorizationService _authService;

    /// <summary>
    /// Department listesi - sadece erişim yetkisi olanlar
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int companyId)
    {
        var currentUserId = GetCurrentUserId();

        // Yetki kontrolü
        if (!await _authService.CanAccessCompanyAsync(currentUserId, companyId))
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        var departments = await _authService.GetUserDepartmentsAsync(currentUserId, companyId);

        return View(departments);
    }

    /// <summary>
    /// Department oluştur - CompanyAdmin yetkisi gerekli
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentDTO dto)
    {
        var currentUserId = GetCurrentUserId();

        // Yetki kontrolü: CompanyAdmin olmalı
        if (!await _authService.IsCompanyAdminAsync(currentUserId, dto.CompanyID))
        {
            AddErrorMessage(T("error.unauthorized"));
            return RedirectToAction("Index", new { companyId = dto.CompanyID });
        }

        // Create işlemi...
        var department = await _departmentService.CreateAsync(dto);

        AddSuccessMessage(T("department.createdSuccess"));
        return RedirectToAction("Index", new { companyId = dto.CompanyID });
    }

    /// <summary>
    /// Department düzenle - CompanyAdmin yetkisi gerekli
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DepartmentDTO dto)
    {
        var currentUserId = GetCurrentUserId();
        var department = await _departmentService.GetByIdAsync(id);

        if (department == null)
            return NotFound();

        // Yetki kontrolü
        if (!await _authService.IsCompanyAdminAsync(currentUserId, department.CompanyID))
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        // Update işlemi...
        await _departmentService.UpdateAsync(id, dto);

        AddSuccessMessage(T("department.updatedSuccess"));
        return RedirectToAction("Index", new { companyId = department.CompanyID });
    }

    /// <summary>
    /// Department sil - CompanyAdmin yetkisi gerekli
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = GetCurrentUserId();
        var department = await _departmentService.GetByIdAsync(id);

        if (department == null)
            return NotFound();

        // Yetki kontrolü
        if (!await _authService.IsCompanyAdminAsync(currentUserId, department.CompanyID))
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        await _departmentService.DeleteAsync(id);

        AddSuccessMessage(T("department.deletedSuccess"));
        return RedirectToAction("Index", new { companyId = department.CompanyID });
    }
}
```

### PageController - Authorization Check

```csharp
[Authorize]
public class PageController : BaseController
{
    private readonly IPageService _pageService;
    private readonly IAuthorizationService _authService;

    /// <summary>
    /// Page listesi - department erişimi olanlar
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int departmentId)
    {
        var currentUserId = GetCurrentUserId();

        // Yetki kontrolü
        if (!await _authService.CanAccessDepartmentAsync(currentUserId, departmentId))
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        var pages = await _pageService.GetByDepartmentIdAsync(departmentId);
        return View(pages);
    }

    /// <summary>
    /// Page oluştur - DepartmentManager yetkisi gerekli
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PageDTO dto)
    {
        var currentUserId = GetCurrentUserId();

        // Yetki kontrolü: DepartmentManager olmalı
        if (!await _authService.IsDepartmentManagerAsync(currentUserId, dto.DepartmentID))
        {
            AddErrorMessage(T("error.unauthorized"));
            return RedirectToAction("Index", new { departmentId = dto.DepartmentID });
        }

        var page = await _pageService.CreateAsync(dto);

        AddSuccessMessage(T("page.createdSuccess"));
        return RedirectToAction("Details", new { id = page.PageID });
    }

    /// <summary>
    /// Page düzenle - DepartmentManager yetkisi gerekli
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PageDTO dto)
    {
        var currentUserId = GetCurrentUserId();

        // Yetki kontrolü
        if (!await _authService.CanModifyPageAsync(currentUserId, id))
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        await _pageService.UpdateAsync(id, dto);

        AddSuccessMessage(T("page.updatedSuccess"));
        return RedirectToAction("Details", new { id });
    }
}
```

---

## Best Practices

### 1. Her Controller Action'da Yetki Kontrolü

```csharp
public async Task<IActionResult> SomeAction(int resourceId)
{
    var currentUserId = GetCurrentUserId();

    // Yetki kontrolü - action başında
    if (!await _authService.CanAccessResourceAsync(currentUserId, resourceId))
    {
        return View("~/Views/Shared/AccessDenied.cshtml");
    }

    // İşlem devam eder...
}
```

### 2. Hiyerarşik Kontrol

```csharp
// En üstten başla
if (await _authService.IsSystemAdminAsync(userId))
    return true; // Tam yetki

// Company seviyesini kontrol et
if (await _authService.IsCompanyAdminAsync(userId, companyId))
    return true; // Şirket yetkisi

// Department seviyesini kontrol et
return await _authService.CanAccessDepartmentAsync(userId, departmentId);
```

### 3. Cache Kullanımı

```csharp
// 10-15 dakika cache
var cacheKey = $"user_{userId}_access_{resourceId}";

if (_cache.TryGetValue(cacheKey, out bool hasAccess))
    return hasAccess;

// Database check...
_cache.Set(cacheKey, hasAccess, TimeSpan.FromMinutes(10));
```

### 4. Audit Logging

```csharp
_logger.LogInformation($"User {userId} {(allowed ? "accessed" : "denied access to")} resource {resourceId}");
```

### 5. Fail-Safe (Güvenli Tarafta Yanılma)

```csharp
// Şüphe durumunda erişimi reddet
if (role == null)
    return false; // ❌ Erişim yok

// Nullable check
var user = await _userService.GetByIdAsync(userId);
if (user == null || !user.IsActive)
    return false; // ❌ Güvenli taraf
```

---

## Usage Examples

### Örnek 1: SystemAdmin Oluşturma

```csharp
var user = new User
{
    UserName = "admin",
    Email = "admin@company.com",
    IsSystemAdmin = true, // ← Platform admini
    IsActive = true
};

await _userService.CreateAsync(user);
```

### Örnek 2: CompanyAdmin Atama

```csharp
// User 5'i Company 3'ün admini yap
await _authService.AssignCompanyRoleAsync(
    userId: 5,
    companyId: 3,
    role: "CompanyAdmin",
    assignedBy: "SystemAdmin"
);

// Artık User 5, Company 3'ün TÜM departmanlarını yönetebilir
```

### Örnek 3: DepartmentManager Atama

```csharp
// User 10'u Department 7 ve 8'in manager'ı yap
await _authService.AssignDepartmentRoleAsync(10, 7, "DepartmentManager", "CompanyAdmin");
await _authService.AssignDepartmentRoleAsync(10, 8, "DepartmentManager", "CompanyAdmin");

// Artık User 10 sadece bu 2 departmanı yönetebilir
```

### Örnek 4: Controller'da Yetki Kontrolü

```csharp
[HttpGet]
public async Task<IActionResult> EditPage(int pageId)
{
    var userId = GetCurrentUserId();

    // Sayfa düzenleme yetkisi var mı?
    if (!await _authService.CanModifyPageAsync(userId, pageId))
    {
        AddErrorMessage(T("error.noPermission"));
        return RedirectToAction("Index");
    }

    var page = await _pageService.GetByIdAsync(pageId);
    return View(page);
}
```

### Örnek 5: Kullanıcının Erişebildiği Şirketleri Listele

```csharp
var userId = GetCurrentUserId();
var companies = await _authService.GetUserCompaniesAsync(userId);

// SystemAdmin ise → TÜM şirketler
// Diğer kullanıcılar → Sadece atandıkları şirketler
```

### Örnek 6: Kullanıcının Erişebildiği Departmanları Listele

```csharp
var userId = GetCurrentUserId();
var companyId = 3;

var departments = await _authService.GetUserDepartmentsAsync(userId, companyId);

// CompanyAdmin ise → O şirketin TÜM departmanları
// DepartmentManager ise → Sadece atandığı departmanlar
```

---

## Migration Example

```csharp
public partial class AddUserDepartmentRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserDepartmentRoles",
            columns: table => new
            {
                UserDepartmentRoleID = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserID = table.Column<int>(nullable: false),
                DepartmentID = table.Column<int>(nullable: false),
                Role = table.Column<string>(maxLength: 50, nullable: false, defaultValue: "Viewer"),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                AssignedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                AssignedBy = table.Column<string>(maxLength: 255, nullable: false, defaultValue: "System")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserDepartmentRoles", x => x.UserDepartmentRoleID);
                table.ForeignKey(
                    name: "FK_UserDepartmentRoles_Users_UserID",
                    column: x => x.UserID,
                    principalTable: "Users",
                    principalColumn: "UserID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserDepartmentRoles_Departments_DepartmentID",
                    column: x => x.DepartmentID,
                    principalTable: "Departments",
                    principalColumn: "DepartmentID",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_UserDepartmentRoles_UserID_DepartmentID",
            table: "UserDepartmentRoles",
            columns: new[] { "UserID", "DepartmentID" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserDepartmentRoles_DepartmentID",
            table: "UserDepartmentRoles",
            column: "DepartmentID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "UserDepartmentRoles");
    }
}
```

---

## Testing Scenarios

### Test 1: SystemAdmin - Full Access
```
User: SystemAdmin (IsSystemAdmin = true)
Expected: Can access ALL companies, departments, pages
```

### Test 2: CompanyAdmin - Company Scope
```
User: User5 (CompanyAdmin of Company 3)
Expected:
  ✅ Can access Company 3
  ✅ Can access ALL departments in Company 3
  ✅ Can manage all pages in Company 3
  ❌ Cannot access Company 1 or 2
```

### Test 3: DepartmentManager - Department Scope
```
User: User10 (DepartmentManager of Dept 7, 8)
Expected:
  ✅ Can view Company 3 (parent of Dept 7, 8)
  ✅ Can access Dept 7 and 8
  ✅ Can manage pages in Dept 7 and 8
  ❌ Cannot access Dept 9 (not assigned)
  ❌ Cannot create new departments
```

### Test 4: Viewer - Read Only
```
User: User15 (Viewer of Company 3)
Expected:
  ✅ Can view Company 3
  ❌ Cannot modify anything
  ❌ Cannot access any department
```

---

## Security Considerations

### 1. **Cascade Permissions**
- CompanyAdmin → Her zaman department erişimi var
- SystemAdmin → Her zaman her şeye erişim var

### 2. **Data Isolation**
- Kullanıcılar sadece yetkili oldukları kaynakları görmeli
- SQL query'lerinde ALWAYS filter by permissions

### 3. **Role Assignment Security**
- Sadece CompanyAdmin veya SystemAdmin rol atayabilir
- Kullanıcı kendi rolünü değiştiremez

### 4. **Cache Invalidation**
- Role değişikliğinde cache temizlenmeli
- User.IsActive değişikliğinde cache temizlenmeli

### 5. **Audit Trail**
- Her role assignment loglanmalı
- AssignedBy alanı doldurulmalı
- DateTime bilgisi saklanmalı

---

## User Management Authorization

### Genel Bakış

Kullanıcı yönetimi sayfası (`/User/Index`), platformdaki en kritik yetkilendirme gereksinimlerine sahip sayfadır. Kullanıcı CRUD işlemleri, rol atamaları ve şirket/departman değişiklikleri için katı yetkilendirme kuralları uygulanır.

### Rol Bazlı Yetkiler

#### 1. Sistem Admin (System Admin)

**Tam Yetki - Tüm İşlemler İzinli**

```
✅ Kullanıcı Görüntüleme:
   - Tüm şirketlerin kullanıcılarını görüntüleyebilir
   - Filtreleme kısıtlaması yok

✅ Kullanıcı CRUD İşlemleri:
   - Yeni kullanıcı oluşturabilir (her şirkete)
   - Kullanıcı bilgilerini düzenleyebilir
   - Kullanıcı silebilir (kısıtlama: son SystemAdmin silinemez)

✅ Şifre Yönetimi:
   - Herhangi bir kullanıcının şifresini sıfırlayabilir
   - Mevcut şifre gerektirmez

✅ Rol Yönetimi:
   - SystemAdmin rolü atayabilir/kaldırabilir
   - CompanyAdmin rolü atayabilir/kaldırabilir
   - DepartmentManager rolü atayabilir/kaldırabilir
   - Standart kullanıcı yapabilir

✅ Şirket/Departman Değişikliği:
   - Kullanıcının şirketini değiştirebilir
   - Kullanıcının departmanını değiştirebilir
   - UserCompanyRole ve UserDepartmentRole yönetebilir
```

**Kısıtlamalar:**
- En az 1 SystemAdmin sistemde kalmalı (son SystemAdmin silinemez veya rolü değiştirilemez)

#### 2. Şirket Admin (Company Admin)

**Sınırlı Yetki - Sadece Kendi Şirketi**

```
✅ Kullanıcı Görüntüleme:
   - SADECE kendi şirketinin kullanıcılarını görüntüleyebilir
   - Başka şirketlerin kullanıcılarını göremez

✅ Kullanıcı CRUD İşlemleri:
   - Kendi şirketine yeni kullanıcı ekleyebilir
   - Kendi şirketindeki kullanıcı bilgilerini düzenleyebilir
   - Kendi şirketindeki kullanıcıları silebilir

✅ Şifre Yönetimi:
   - Kendi şirketindeki kullanıcıların şifresini sıfırlayabilir
   - Mevcut şifre gerektirmez

✅ Rol Yönetimi:
   - CompanyAdmin rolü atayabilir/kaldırabilir (kendi şirketinde)
   - DepartmentManager rolü atayabilir/kaldırabilir (kendi şirketinde)
   - Standart kullanıcı yapabilir

✅ Departman Değişikliği:
   - Kullanıcının departmanını değiştirebilir (kendi şirketi içinde)
   - UserDepartmentRole yönetebilir (kendi şirketi için)
```

**Kısıtlamalar:**
- ❌ Başka şirketlerin kullanıcılarını göremez
- ❌ SystemAdmin rolü atayamaz
- ❌ Kullanıcının şirketini değiştiremez (sadece kendi şirketi)
- ❌ Başka şirketlere kullanıcı ekleyemez

#### 3. Departman Yetkilisi (Department Manager)

**Kullanıcı Yönetimine Erişim YOK**

```
❌ Kullanıcı yönetimi sayfasına erişim yok (/User/Index)
❌ Hiçbir kullanıcı CRUD işlemi yapamaz
❌ Rol atayamaz
❌ Şifre değiştiremez (kendi şifresi hariç)
```

**Sadece Kendi Profili:**
- ✅ Kendi profilini görüntüleyebilir (`/Account/Profile`)
- ✅ Kendi şifresini değiştirebilir (`/Account/ChangePassword`)

#### 4. Standart Kullanıcı (Standard User / Viewer)

**Kullanıcı Yönetimine Erişim YOK**

```
❌ Kullanıcı yönetimi sayfasına erişim yok (/User/Index)
❌ Hiçbir kullanıcı CRUD işlemi yapamaz
❌ Rol atayamaz
❌ Şifre değiştiremez (kendi şifresi hariç)
```

**Sadece Kendi Profili:**
- ✅ Kendi profilini görüntüleyebilir (`/Account/Profile`)
- ✅ Kendi şifresini değiştirebilir (`/Account/ChangePassword`)

---

### Yetki Kontrolü Akış Diyagramı

```
┌─────────────────────────────────────┐
│  User Management Access Request    │
│     (/User/Index)                   │
└──────────────┬──────────────────────┘
               │
               ▼
      ┌────────────────┐
      │ IsSystemAdmin? │
      └────────┬───────┘
          Yes │ No
    ┌─────────┴──────────┐
    ▼                    ▼
┌─────────┐     ┌─────────────────┐
│  ALLOW  │     │ IsCompanyAdmin? │
│(All Co) │     └────────┬────────┘
└─────────┘          Yes │ No
              ┌──────────┴──────────┐
              ▼                     ▼
       ┌─────────────┐        ┌─────────┐
       │   ALLOW     │        │  DENY   │
       │(Own Company)│        │(403)    │
       └─────────────┘        └─────────┘
```

### Kullanıcı Filtreleme Kuralları

#### SystemAdmin - Tüm Kullanıcılar

```csharp
public async Task<IActionResult> Index()
{
    var currentUserId = GetCurrentUserId();

    if (await _authService.IsSystemAdminAsync(currentUserId))
    {
        // Tüm kullanıcıları getir (filtreleme yok)
        var allUsers = await _userService.GetAllAsync();
        return View(allUsers);
    }

    // ...
}
```

#### CompanyAdmin - Sadece Kendi Şirketi

```csharp
public async Task<IActionResult> Index()
{
    var currentUserId = GetCurrentUserId();

    // Current user'ın şirketini al
    var currentUser = await _userService.GetByIdAsync(currentUserId);
    var userCompanyRoles = await _authService.GetUserCompanyRolesAsync(currentUserId);

    // CompanyAdmin olduğu şirketleri bul
    var adminCompanyIds = userCompanyRoles
        .Where(ucr => ucr.Role == "CompanyAdmin")
        .Select(ucr => ucr.CompanyID)
        .ToList();

    if (!adminCompanyIds.Any())
    {
        // CompanyAdmin değilse erişim reddet
        return View("~/Views/Shared/AccessDenied.cshtml");
    }

    // Sadece kendi şirketlerinin kullanıcılarını getir
    var users = await _userService.GetUsersByCompanyIdsAsync(adminCompanyIds);
    return View(users);
}
```

---

### CRUD İşlem Yetkileri

#### Create User (Yeni Kullanıcı)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(UserDTO dto)
{
    var currentUserId = GetCurrentUserId();

    // SystemAdmin her şirkete kullanıcı ekleyebilir
    if (await _authService.IsSystemAdminAsync(currentUserId))
    {
        var user = await _userService.CreateAsync(dto);
        AddSuccessMessage(T("user.created"));
        return RedirectToAction("Index");
    }

    // CompanyAdmin sadece kendi şirketine ekleyebilir
    if (await _authService.IsCompanyAdminAsync(currentUserId, dto.CompanyID))
    {
        // SystemAdmin rolü atanamaz
        if (dto.IsSystemAdmin)
        {
            AddErrorMessage(T("user.cannotAssignSystemAdmin"));
            return View(dto);
        }

        var user = await _userService.CreateAsync(dto);
        AddSuccessMessage(T("user.created"));
        return RedirectToAction("Index");
    }

    // Yetkisiz
    AddErrorMessage(T("error.unauthorized"));
    return RedirectToAction("Index");
}
```

#### Edit User (Kullanıcı Düzenle)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, UserDTO dto)
{
    var currentUserId = GetCurrentUserId();
    var targetUser = await _userService.GetByIdAsync(id);

    if (targetUser == null)
        return NotFound();

    // SystemAdmin her kullanıcıyı düzenleyebilir
    if (await _authService.IsSystemAdminAsync(currentUserId))
    {
        await _userService.UpdateAsync(id, dto);
        AddSuccessMessage(T("user.updated"));
        return RedirectToAction("Index");
    }

    // CompanyAdmin sadece kendi şirketindeki kullanıcıları düzenleyebilir
    var targetUserCompanies = await _authService.GetUserCompanyIdsAsync(id);
    var currentUserAdminCompanies = await _authService.GetAdminCompanyIdsAsync(currentUserId);

    // Target user'ın şirketi, current user'ın admin olduğu şirketlerden biri mi?
    if (targetUserCompanies.Any(c => currentUserAdminCompanies.Contains(c)))
    {
        // SystemAdmin rolü atayamaz
        if (dto.IsSystemAdmin && !targetUser.IsSystemAdmin)
        {
            AddErrorMessage(T("user.cannotAssignSystemAdmin"));
            return View(dto);
        }

        // Şirket değiştiremez
        if (dto.CompanyID != targetUser.CompanyID)
        {
            AddErrorMessage(T("user.cannotChangeCompany"));
            return View(dto);
        }

        await _userService.UpdateAsync(id, dto);
        AddSuccessMessage(T("user.updated"));
        return RedirectToAction("Index");
    }

    // Yetkisiz
    AddErrorMessage(T("error.unauthorized"));
    return RedirectToAction("Index");
}
```

#### Delete User (Kullanıcı Sil)

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var currentUserId = GetCurrentUserId();
    var targetUser = await _userService.GetByIdAsync(id);

    if (targetUser == null)
        return NotFound();

    // Kendini silemez
    if (id == currentUserId)
    {
        AddErrorMessage(T("user.cannotDeleteSelf"));
        return RedirectToAction("Index");
    }

    // Son SystemAdmin silinemez
    if (targetUser.IsSystemAdmin)
    {
        var systemAdminCount = await _userService.CountSystemAdminsAsync();
        if (systemAdminCount <= 1)
        {
            AddErrorMessage(T("user.cannotDeleteLastSystemAdmin"));
            return RedirectToAction("Index");
        }
    }

    // SystemAdmin her kullanıcıyı silebilir
    if (await _authService.IsSystemAdminAsync(currentUserId))
    {
        await _userService.DeleteAsync(id);
        AddSuccessMessage(T("user.deleted"));
        return RedirectToAction("Index");
    }

    // CompanyAdmin sadece kendi şirketindeki kullanıcıları silebilir
    var targetUserCompanies = await _authService.GetUserCompanyIdsAsync(id);
    var currentUserAdminCompanies = await _authService.GetAdminCompanyIdsAsync(currentUserId);

    if (targetUserCompanies.Any(c => currentUserAdminCompanies.Contains(c)))
    {
        await _userService.DeleteAsync(id);
        AddSuccessMessage(T("user.deleted"));
        return RedirectToAction("Index");
    }

    // Yetkisiz
    AddErrorMessage(T("error.unauthorized"));
    return RedirectToAction("Index");
}
```

---

### View-Level Authorization

#### Index View - Buton Görünürlüğü

```razor
@{
    var isSystemAdmin = User.FindFirst("IsSystemAdmin")?.Value == "True";
    var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}

<!-- Create Button - Sadece SystemAdmin ve CompanyAdmin -->
@if (isSystemAdmin || ViewBag.IsCompanyAdmin)
{
    <a asp-action="Create" class="btn btn-primary">
        <i class="fas fa-plus"></i> @T("user.create")
    </a>
}

<!-- User List -->
@foreach (var user in Model)
{
    <div class="user-row">
        <span>@user.UserName</span>

        <!-- Actions Dropdown -->
        @if (isSystemAdmin || (ViewBag.IsCompanyAdmin && ViewBag.UserCompanyIds.Contains(user.CompanyID)))
        {
            <div class="dropdown">
                <!-- Edit -->
                <a asp-action="Edit" asp-route-id="@user.UserID">
                    <i class="fas fa-edit"></i> @T("common.edit")
                </a>

                <!-- Change Password -->
                <a asp-controller="Account" asp-action="ChangePassword" asp-route-userId="@user.UserID">
                    <i class="fas fa-key"></i> @T("user.changePassword")
                </a>

                <!-- Delete (kendisi değilse) -->
                @if (user.UserID != currentUserId)
                {
                    <a asp-action="Delete" asp-route-id="@user.UserID">
                        <i class="fas fa-trash"></i> @T("common.delete")
                    </a>
                }
            </div>
        }
    </div>
}
```

#### Edit View - Rol Seçenekleri

```razor
@{
    var isSystemAdmin = User.FindFirst("IsSystemAdmin")?.Value == "True";
}

<!-- SystemAdmin Checkbox - Sadece SystemAdmin görebilir -->
@if (isSystemAdmin)
{
    <div class="form-check">
        <input type="checkbox" asp-for="IsSystemAdmin" class="form-check-input" />
        <label asp-for="IsSystemAdmin" class="form-check-label">
            @T("user.isSystemAdmin")
        </label>
    </div>
}

<!-- Company Dropdown -->
<div class="mb-3">
    <label asp-for="CompanyID" class="form-label">@T("user.company")</label>
    <select asp-for="CompanyID" class="form-select" @(isSystemAdmin ? "" : "disabled")>
        @foreach (var company in ViewBag.Companies)
        {
            <option value="@company.CompanyID">@company.CompanyName</option>
        }
    </select>
    @if (!isSystemAdmin)
    {
        <small class="text-muted">@T("user.companyCannotBeChangedByCompanyAdmin")</small>
    }
</div>
```

---

### Service Layer Methods

#### IUserService - Yeni Metodlar

```csharp
public interface IUserService
{
    // Existing methods...

    // User Management Authorization
    Task<int> CountSystemAdminsAsync();
    Task<List<User>> GetUsersByCompanyIdsAsync(List<int> companyIds);
    Task<List<int>> GetUserCompanyIdsAsync(int userId);
    Task<List<int>> GetAdminCompanyIdsAsync(int userId);
}
```

#### UserService Implementation

```csharp
public async Task<int> CountSystemAdminsAsync()
{
    var users = await _unitOfWork.Users.FindAsync(u => u.IsSystemAdmin && u.IsActive);
    return users.Count;
}

public async Task<List<User>> GetUsersByCompanyIdsAsync(List<int> companyIds)
{
    var allUsers = await _unitOfWork.Users.GetAllAsync();

    var filteredUsers = new List<User>();

    foreach (var user in allUsers)
    {
        var userCompanyRoles = await _unitOfWork.UserCompanyRoles.FindAsync(
            ucr => ucr.UserID == user.UserID && ucr.IsActive
        );

        // Kullanıcının şirketlerinden biri, filtre listesinde mi?
        if (userCompanyRoles.Any(ucr => companyIds.Contains(ucr.CompanyID)))
        {
            filteredUsers.Add(user);
        }
    }

    return filteredUsers;
}

public async Task<List<int>> GetUserCompanyIdsAsync(int userId)
{
    var companyRoles = await _unitOfWork.UserCompanyRoles.FindAsync(
        ucr => ucr.UserID == userId && ucr.IsActive
    );

    return companyRoles.Select(ucr => ucr.CompanyID).Distinct().ToList();
}

public async Task<List<int>> GetAdminCompanyIdsAsync(int userId)
{
    // SystemAdmin ise tüm şirketleri döndür
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
    if (user?.IsSystemAdmin == true)
    {
        var allCompanies = await _unitOfWork.Companies.GetAllAsync();
        return allCompanies.Select(c => c.CompanyID).ToList();
    }

    // CompanyAdmin olduğu şirketleri döndür
    var companyRoles = await _unitOfWork.UserCompanyRoles.FindAsync(
        ucr => ucr.UserID == userId && ucr.IsActive && ucr.Role == "CompanyAdmin"
    );

    return companyRoles.Select(ucr => ucr.CompanyID).Distinct().ToList();
}
```

---

### Güvenlik Kontrolleri (Critical)

#### 1. Son SystemAdmin Koruması

```csharp
// DELETE veya ROLE CHANGE öncesi kontrol
var systemAdminCount = await _userService.CountSystemAdminsAsync();
if (targetUser.IsSystemAdmin && systemAdminCount <= 1)
{
    throw new InvalidOperationException("Cannot delete or demote the last System Admin");
}
```

#### 2. Kendi Rolünü Değiştirememe

```csharp
// ROLE CHANGE öncesi kontrol
if (targetUserId == currentUserId && dto.IsSystemAdmin != currentUser.IsSystemAdmin)
{
    AddErrorMessage(T("user.cannotChangeOwnRole"));
    return View(dto);
}
```

#### 3. CompanyAdmin - SystemAdmin Atama Engeli

```csharp
// CREATE/EDIT öncesi kontrol (CompanyAdmin için)
if (!await _authService.IsSystemAdminAsync(currentUserId) && dto.IsSystemAdmin)
{
    AddErrorMessage(T("user.cannotAssignSystemAdmin"));
    return View(dto);
}
```

#### 4. CompanyAdmin - Şirket Değişikliği Engeli

```csharp
// EDIT öncesi kontrol (CompanyAdmin için)
if (!await _authService.IsSystemAdminAsync(currentUserId))
{
    if (dto.CompanyID != originalUser.CompanyID)
    {
        AddErrorMessage(T("user.cannotChangeCompany"));
        return View(dto);
    }
}
```

---

### Translation Keys

#### Yeni Dil Anahtarları (tr.json, en.json, de.json)

```json
{
  "user.cannotAssignSystemAdmin": "Sistem Admin rolü atama yetkiniz yok",
  "user.cannotChangeCompany": "Kullanıcının şirketini değiştirme yetkiniz yok",
  "user.cannotDeleteSelf": "Kendi kullanıcınızı silemezsiniz",
  "user.cannotDeleteLastSystemAdmin": "Son Sistem Admin kullanıcısı silinemez",
  "user.cannotChangeOwnRole": "Kendi rolünüzü değiştiremezsiniz",
  "user.companyCannotBeChangedByCompanyAdmin": "Şirket Adminleri kullanıcının şirketini değiştiremez"
}
```

---

### Test Scenarios

#### Senaryo 1: SystemAdmin - Tam Yetki
```
User: SystemAdmin (IsSystemAdmin = true)
Action: View /User/Index
Expected: ✅ Tüm şirketlerin tüm kullanıcılarını görebilir
Expected: ✅ Create/Edit/Delete butonları görünür
Expected: ✅ Her kullanıcının şifresini değiştirebilir
```

#### Senaryo 2: CompanyAdmin - Kendi Şirketi
```
User: CompanyAdmin (Company 3)
Action: View /User/Index
Expected: ✅ Sadece Company 3'ün kullanıcılarını görebilir
Expected: ❌ Company 1 ve 2'nin kullanıcılarını göremez
Expected: ✅ Create/Edit/Delete butonları görünür (Company 3 için)
Expected: ❌ SystemAdmin rolü atayamaz
```

#### Senaryo 3: CompanyAdmin - Şirket Değiştirme
```
User: CompanyAdmin (Company 3)
Action: Edit User (Company 3 user) → Change CompanyID to Company 2
Expected: ❌ İşlem başarısız (CompanyAdmin şirket değiştiremez)
Error: "user.cannotChangeCompany"
```

#### Senaryo 4: Son SystemAdmin Silme
```
User: SystemAdmin (only 1 SystemAdmin in database)
Action: Delete self or demote role
Expected: ❌ İşlem başarısız (Son SystemAdmin koruması)
Error: "user.cannotDeleteLastSystemAdmin"
```

#### Senaryo 5: DepartmentManager - Erişim Reddi
```
User: DepartmentManager (Dept 5)
Action: Navigate to /User/Index
Expected: ❌ 403 Forbidden / AccessDenied view
```

#### Senaryo 6: Viewer - Erişim Reddi
```
User: Viewer (Company 3)
Action: Navigate to /User/Index
Expected: ❌ 403 Forbidden / AccessDenied view
```

---

## References

- [ASP.NET Core Authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/)
- [Role-Based Access Control (RBAC)](https://en.wikipedia.org/wiki/Role-based_access_control)
- [Multi-Tenant Security Best Practices](https://learn.microsoft.com/en-us/azure/architecture/multitenant/security/overview)
