using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetActiveUsersAsync();
    }
}
