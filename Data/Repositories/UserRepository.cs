using DigitalSignage.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await FindAsync(u => u.IsActive);
        }
    }
}
