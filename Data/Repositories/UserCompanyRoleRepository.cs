using DigitalSignage.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class UserCompanyRoleRepository : Repository<UserCompanyRole>, IUserCompanyRoleRepository
    {
        public UserCompanyRoleRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<UserCompanyRole>> GetRolesByUserAsync(int userId)
        {
            return await _dbSet.AsNoTracking()
                .Where(ucr => ucr.UserID == userId && ucr.IsActive)
                .Include(ucr => ucr.Company)
                .OrderBy(ucr => ucr.Company.CompanyName)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserCompanyRole>> GetRolesByCompanyAsync(int companyId)
        {
            return await _dbSet.AsNoTracking()
                .Where(ucr => ucr.CompanyID == companyId && ucr.IsActive)
                .Include(ucr => ucr.User)
                .OrderBy(ucr => ucr.User.UserName)
                .ToListAsync();
        }

        public async Task<UserCompanyRole?> GetUserRoleInCompanyAsync(int userId, int companyId)
        {
            return await _dbSet
                .Include(ucr => ucr.User)
                .Include(ucr => ucr.Company)
                .FirstOrDefaultAsync(ucr =>
                    ucr.UserID == userId &&
                    ucr.CompanyID == companyId &&
                    ucr.IsActive);
        }

        public async Task<IEnumerable<UserCompanyRole>> GetUsersByRoleAsync(int companyId, string role)
        {
            return await _dbSet.AsNoTracking()
                .Where(ucr => ucr.CompanyID == companyId && ucr.Role == role && ucr.IsActive)
                .Include(ucr => ucr.User)
                .OrderBy(ucr => ucr.User.UserName)
                .ToListAsync();
        }

        public async Task<bool> HasRoleAsync(int userId, int companyId, string role)
        {
            return await _dbSet.AnyAsync(ucr =>
                ucr.UserID == userId &&
                ucr.CompanyID == companyId &&
                ucr.Role == role &&
                ucr.IsActive);
        }

        public async Task<string?> GetUserRoleAsync(int userId, int companyId)
        {
            var role = await _dbSet.AsNoTracking()
                .Where(ucr => ucr.UserID == userId && ucr.CompanyID == companyId && ucr.IsActive)
                .Select(ucr => ucr.Role)
                .FirstOrDefaultAsync();

            return role;
        }
    }
}
