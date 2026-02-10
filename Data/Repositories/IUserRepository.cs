using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<User?> GetUserWithRolesAsync(int userId);
        Task<IEnumerable<User>> GetUsersByCompanyAsync(int companyId);
        Task<PagedResult<User>> GetUsersPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<bool> IsUserNameTakenAsync(string userName, int? excludeUserId = null);
        Task<bool> IsEmailTakenAsync(string email, int? excludeUserId = null);
    }
}
