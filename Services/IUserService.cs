using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Services
{
    public interface IUserService : IService<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> AuthenticateAsync(string email, string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<User?> GetUserWithRolesAsync(int userId);
        Task<IEnumerable<User>> GetUsersByCompanyAsync(int companyId);
        Task<PagedResult<User>> GetUsersPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);

        // User Management Authorization
        Task<int> CountSystemAdminsAsync();
        Task<int> CountActiveSystemAdminsAsync();
        Task<List<User>> GetUsersByCompanyIdsAsync(List<int> companyIds);
        Task<List<int>> GetUserCompanyIdsAsync(int userId);
        Task<List<int>> GetAdminCompanyIdsAsync(int userId);
    }
}
