using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public interface IUserCompanyRoleRepository : IRepository<UserCompanyRole>
    {
        Task<IEnumerable<UserCompanyRole>> GetRolesByUserAsync(int userId);
        Task<IEnumerable<UserCompanyRole>> GetRolesByCompanyAsync(int companyId);
        Task<UserCompanyRole?> GetUserRoleInCompanyAsync(int userId, int companyId);
        Task<IEnumerable<UserCompanyRole>> GetUsersByRoleAsync(int companyId, string role);
        Task<bool> HasRoleAsync(int userId, int companyId, string role);
        Task<string?> GetUserRoleAsync(int userId, int companyId);
    }
}
