using DigitalSignage.Data;
using DigitalSignage.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalSignage.Services
{
    /// <summary>
    /// Authorization service implementation - tüm yetkilendirme kontrollerini yönetir
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthorizationService> _logger;

        // Kullanıcı bazlı cache key takibi: userId → set of cache keys
        private static readonly ConcurrentDictionary<int, HashSet<string>> _userCacheKeys = new();
        private static readonly object _keyLock = new();

        public AuthorizationService(
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            ILogger<AuthorizationService> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        private void TrackCacheKey(int userId, string key)
        {
            _userCacheKeys.AddOrUpdate(
                userId,
                _ => new HashSet<string> { key },
                (_, existing) => { lock (_keyLock) { existing.Add(key); } return existing; }
            );
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
            TrackCacheKey(userId, cacheKey);
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
            TrackCacheKey(userId, cacheKey);

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
                var allCompanies = await _unitOfWork.Companies.GetAllAsync();
                return allCompanies.Where(c => c.IsActive).ToList();
            }

            var companyRoles = await _unitOfWork.UserCompanyRoles.FindAsync(
                ucr => ucr.UserID == userId && ucr.IsActive
            );

            var companyIds = companyRoles.Select(ucr => ucr.CompanyID).Distinct().ToList();

            // FİX N+1: Batch query ile tüm şirketleri tek sorguda getir
            var companies = await _unitOfWork.Companies.Query()
                .Where(c => companyIds.Contains(c.CompanyID) && c.IsActive)
                .ToListAsync();

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

        public async Task<bool> CanEditInDepartmentAsync(int userId, int departmentId)
        {
            // DepartmentManager'ın tüm yetkilerine ek olarak Editor da Create/Edit yapabilir
            if (await IsDepartmentManagerAsync(userId, departmentId))
                return true;

            var role = await GetDepartmentRoleAsync(userId, departmentId);
            return role?.Role == "Editor";
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
                var allDepts = await _unitOfWork.Departments.FindWithIncludesAsync(
                    d => d.CompanyID == companyId && d.IsActive,
                    d => d.Company
                );
                return allDepts.ToList();
            }

            // Kullanıcının atandığı departmanları al
            var departmentRoles = await _unitOfWork.UserDepartmentRoles.FindAsync(
                udr => udr.UserID == userId && udr.IsActive
            );

            var departmentIds = departmentRoles.Select(udr => udr.DepartmentID).ToList();

            // Batch query ile tüm departmanları Company bilgisiyle tek sorguda getir
            var departments = await _unitOfWork.Departments.Query()
                .Include(d => d.Company)
                .Where(d => departmentIds.Contains(d.DepartmentID) && d.CompanyID == companyId && d.IsActive)
                .ToListAsync();

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

        public async Task<bool> CanEditPageAsync(int userId, int pageId)
        {
            var page = await _unitOfWork.Pages.GetByIdAsync(pageId);
            if (page == null)
                return false;

            return await CanEditInDepartmentAsync(userId, page.DepartmentID);
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

            await _unitOfWork.SaveChangesAsync();

            // Cache temizle
            ClearUserCache(userId);

            _logger.LogInformation($"User {userId} assigned role '{role}' for company {companyId} by {assignedBy}");
        }

        public async Task RemoveCompanyRoleAsync(int userId, int companyId)
        {
            var role = await GetCompanyRoleAsync(userId, companyId);
            if (role != null)
            {
                // Önce o şirketteki tüm department rollerini kaldır
                var departments = await _unitOfWork.Departments.FindAsync(d => d.CompanyID == companyId);
                foreach (var dept in departments)
                {
                    var deptRole = await GetDepartmentRoleAsync(userId, dept.DepartmentID);
                    if (deptRole != null)
                    {
                        await _unitOfWork.UserDepartmentRoles.DeleteAsync(deptRole.UserDepartmentRoleID);
                        _logger.LogInformation("Cascade removed department role for user {UserId} from department {DepartmentId} (company role removed)", userId, dept.DepartmentID);
                    }
                }

                // Sonra company role'ü kaldır
                await _unitOfWork.UserCompanyRoles.DeleteAsync(role.UserCompanyRoleID);
                await _unitOfWork.SaveChangesAsync();

                ClearUserCache(userId);

                _logger.LogInformation("User {UserId} removed from company {CompanyId} (with cascade department cleanup)", userId, companyId);
            }
        }

        public async Task AssignDepartmentRoleAsync(int userId, int departmentId, string role, string assignedBy)
        {
            // Departmanın şirketini bul ve CompanyRole yoksa otomatik "Viewer" ata
            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department != null)
            {
                var companyRole = await GetCompanyRoleAsync(userId, department.CompanyID);
                if (companyRole == null)
                {
                    await AssignCompanyRoleAsync(userId, department.CompanyID, "Viewer", assignedBy);
                    _logger.LogInformation("Auto-assigned Company Viewer role for user {UserId} in company {CompanyId} (triggered by department role assignment)", userId, department.CompanyID);
                }
            }

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

            await _unitOfWork.SaveChangesAsync();

            ClearUserCache(userId);

            _logger.LogInformation($"User {userId} assigned role '{role}' for department {departmentId} by {assignedBy}");
        }

        public async Task RemoveDepartmentRoleAsync(int userId, int departmentId)
        {
            var role = await GetDepartmentRoleAsync(userId, departmentId);
            if (role != null)
            {
                await _unitOfWork.UserDepartmentRoles.DeleteAsync(role.UserDepartmentRoleID);
                await _unitOfWork.SaveChangesAsync();

                ClearUserCache(userId);

                _logger.LogInformation($"User {userId} removed from department {departmentId}");
            }
        }

        // ============== ROLE QUERIES ==============

        public async Task<bool> HasAnyRoleAsync(int userId)
        {
            if (await IsSystemAdminAsync(userId))
                return true;

            var companyRoles = await _unitOfWork.UserCompanyRoles.FindAsync(
                ucr => ucr.UserID == userId && ucr.IsActive
            );
            return companyRoles.Any();
        }

        public async Task<bool> HasAnyCompanyAdminRoleAsync(int userId)
        {
            if (await IsSystemAdminAsync(userId))
                return true;

            var companyAdminRoles = await _unitOfWork.UserCompanyRoles.FindAsync(
                ucr => ucr.UserID == userId && ucr.Role == "CompanyAdmin" && ucr.IsActive
            );
            return companyAdminRoles.Any();
        }

        // ============== CACHE MANAGEMENT ==============

        public void ClearUserCache(int userId)
        {
            if (_userCacheKeys.TryRemove(userId, out var keys))
            {
                lock (_keyLock)
                {
                    foreach (var key in keys)
                        _cache.Remove(key);
                }

                _logger.LogInformation("User cache cleared for userId={UserId}. Removed {Count} cache entries.", userId, keys.Count);
            }
            else
            {
                // Takipli key yoksa en azından SystemAdmin key'ini temizle
                _cache.Remove($"user_sysadmin_{userId}");
                _logger.LogInformation("User cache cleared for userId={UserId} (sysadmin key only).", userId);
            }
        }
    }
}
