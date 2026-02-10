using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class ScheduleRepository : Repository<Schedule>, IScheduleRepository
    {
        public ScheduleRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Schedule>> GetSchedulesByDepartmentAsync(int departmentId)
        {
            return await _dbSet.AsNoTracking()
                .Where(s => s.DepartmentID == departmentId)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<Schedule?> GetScheduleWithPagesAsync(int scheduleId)
        {
            return await _dbSet
                .Include(s => s.SchedulePages.OrderBy(sp => sp.DisplayOrder))
                    .ThenInclude(sp => sp.Page)
                        .ThenInclude(p => p.Layout)
                .FirstOrDefaultAsync(s => s.ScheduleID == scheduleId);
        }

        public async Task<IEnumerable<Schedule>> GetActiveSchedulesByDepartmentAsync(int departmentId)
        {
            return await _dbSet.AsNoTracking()
                .Where(s => s.DepartmentID == departmentId && s.IsActive)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetSchedulesByDateRangeAsync(
            int departmentId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet.AsNoTracking()
                .Where(s => s.DepartmentID == departmentId &&
                            s.StartDate <= endDate &&
                            s.EndDate >= startDate)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetCurrentActiveSchedulesAsync(int departmentId)
        {
            var now = DateTime.UtcNow;
            var currentTime = now.TimeOfDay;

            return await _dbSet.AsNoTracking()
                .Where(s => s.DepartmentID == departmentId &&
                            s.IsActive &&
                            s.StartDate <= now &&
                            s.EndDate >= now &&
                            s.StartTime <= currentTime &&
                            s.EndTime >= currentTime)
                .Include(s => s.SchedulePages.OrderBy(sp => sp.DisplayOrder))
                    .ThenInclude(sp => sp.Page)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Schedule>> GetSchedulesPagedAsync(
            int departmentId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            IQueryable<Schedule> query = _dbSet.AsNoTracking()
                .Where(s => s.DepartmentID == departmentId);

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
                .OrderBy(s => s.StartDate)
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
