using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class ScheduleRepository : Repository<Schedule>, IScheduleRepository
    {
        public ScheduleRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Schedule>> GetSchedulesByCompanyAsync(int companyId)
        {
            return await _dbSet.AsNoTracking()
                .Where(s => s.CompanyID == companyId)
                .Include(s => s.SchedulePages)
                .OrderBy(s => s.ScheduleName)
                .ToListAsync();
        }

        public async Task<Schedule?> GetScheduleWithPagesAsync(int scheduleId)
        {
            return await _dbSet
                .Include(s => s.Company)
                .Include(s => s.SchedulePages.OrderBy(sp => sp.DisplayOrder))
                    .ThenInclude(sp => sp.Page)
                        .ThenInclude(p => p.Layout)
                .Include(s => s.SchedulePages.OrderBy(sp => sp.DisplayOrder))
                    .ThenInclude(sp => sp.Page)
                        .ThenInclude(p => p.Department)
                .FirstOrDefaultAsync(s => s.ScheduleID == scheduleId);
        }

        public async Task<IEnumerable<Schedule>> GetActiveSchedulesByCompanyAsync(int companyId)
        {
            return await _dbSet.AsNoTracking()
                .Where(s => s.CompanyID == companyId && s.IsActive)
                .Include(s => s.SchedulePages.OrderBy(sp => sp.DisplayOrder))
                    .ThenInclude(sp => sp.Page)
                .OrderBy(s => s.ScheduleName)
                .ToListAsync();
        }

        public async Task<PagedResult<Schedule>> GetSchedulesPagedAsync(
            int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            IQueryable<Schedule> query = _dbSet.AsNoTracking()
                .Where(s => s.CompanyID == companyId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(s =>
                    s.ScheduleName.ToLower().Contains(term));
            }

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(s => s.SchedulePages)
                .OrderBy(s => s.ScheduleName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Schedule>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
