using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet.AsNoTracking()
                .Where(u => u.IsActive)
                .OrderBy(u => u.Email)
                .ToListAsync();
        }

        public async Task<User?> GetUserWithRolesAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.UserCompanyRoles)
                    .ThenInclude(ucr => ucr.Company)
                .FirstOrDefaultAsync(u => u.UserID == userId);
        }

        public async Task<IEnumerable<User>> GetUsersByCompanyAsync(int companyId)
        {
            return await _dbSet.AsNoTracking()
                .Where(u => u.UserCompanyRoles.Any(ucr => ucr.CompanyID == companyId && ucr.IsActive))
                .Include(u => u.UserCompanyRoles.Where(ucr => ucr.CompanyID == companyId))
                .OrderBy(u => u.Email)
                .ToListAsync();
        }

        public async Task<PagedResult<User>> GetUsersPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            IQueryable<User> query = _dbSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(term) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(term)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(term)));
            }

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.Email)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<User>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> IsEmailTakenAsync(string email, int? excludeUserId = null)
        {
            return await _dbSet.AnyAsync(u =>
                u.Email == email &&
                (!excludeUserId.HasValue || u.UserID != excludeUserId.Value));
        }
    }
}
