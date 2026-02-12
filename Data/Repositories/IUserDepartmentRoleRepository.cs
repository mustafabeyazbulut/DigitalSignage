using DigitalSignage.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalSignage.Data.Repositories
{
    /// <summary>
    /// UserDepartmentRole repository interface
    /// </summary>
    public interface IUserDepartmentRoleRepository : IRepository<UserDepartmentRole>
    {
        /// <summary>
        /// Kullanıcının tüm departman rollerini getirir
        /// </summary>
        Task<List<UserDepartmentRole>> GetUserDepartmentRolesAsync(int userId);

        /// <summary>
        /// Departmandaki tüm kullanıcı rollerini getirir
        /// </summary>
        Task<List<UserDepartmentRole>> GetDepartmentUsersAsync(int departmentId);

        /// <summary>
        /// Kullanıcının belirli bir departmandaki rolünü getirir
        /// </summary>
        Task<UserDepartmentRole?> GetUserDepartmentRoleAsync(int userId, int departmentId);

        /// <summary>
        /// Kullanıcının bir departmana erişimi var mı kontrol eder
        /// </summary>
        Task<bool> CheckUserDepartmentAccessAsync(int userId, int departmentId);

        /// <summary>
        /// Kullanıcıya departman rolü atar
        /// </summary>
        Task<UserDepartmentRole> AssignRoleAsync(int userId, int departmentId, string role, string assignedBy);

        /// <summary>
        /// Kullanıcının departman rolünü kaldırır
        /// </summary>
        Task<bool> RemoveRoleAsync(int userId, int departmentId);
    }
}
