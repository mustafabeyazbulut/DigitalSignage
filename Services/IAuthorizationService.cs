using DigitalSignage.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalSignage.Services
{
    /// <summary>
    /// Authorization service interface - yetkilendirme kontrolü
    /// </summary>
    public interface IAuthorizationService
    {
        // ============== SYSTEM LEVEL ==============
        Task<bool> IsSystemAdminAsync(int userId);

        // ============== COMPANY LEVEL ==============
        Task<bool> CanAccessCompanyAsync(int userId, int companyId);
        Task<bool> IsCompanyAdminAsync(int userId, int companyId);
        Task<List<Company>> GetUserCompaniesAsync(int userId);
        Task<UserCompanyRole?> GetCompanyRoleAsync(int userId, int companyId);

        // ============== DEPARTMENT LEVEL ==============
        Task<bool> CanAccessDepartmentAsync(int userId, int departmentId);
        Task<bool> IsDepartmentManagerAsync(int userId, int departmentId);
        Task<bool> CanEditInDepartmentAsync(int userId, int departmentId);
        Task<List<Department>> GetUserDepartmentsAsync(int userId, int companyId);
        Task<UserDepartmentRole?> GetDepartmentRoleAsync(int userId, int departmentId);

        // ============== PAGE LEVEL ==============
        Task<bool> CanAccessPageAsync(int userId, int pageId);
        Task<bool> CanEditPageAsync(int userId, int pageId);
        Task<bool> CanModifyPageAsync(int userId, int pageId);

        // ============== ROLE ASSIGNMENT ==============
        Task AssignCompanyRoleAsync(int userId, int companyId, string role, string assignedBy);
        Task RemoveCompanyRoleAsync(int userId, int companyId);
        Task AssignDepartmentRoleAsync(int userId, int departmentId, string role, string assignedBy);
        Task RemoveDepartmentRoleAsync(int userId, int departmentId);

        // ============== ROLE QUERIES ==============
        Task<bool> HasAnyRoleAsync(int userId);
        Task<bool> HasAnyCompanyAdminRoleAsync(int userId);

        // ============== CACHE MANAGEMENT ==============
        void ClearUserCache(int userId);
    }
}
