using DigitalSignage.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class SchedulePageRepository : Repository<SchedulePage>, ISchedulePageRepository
    {
        public SchedulePageRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<SchedulePage>> GetPagesByScheduleAsync(int scheduleId)
        {
            return await _dbSet.AsNoTracking()
                .Where(sp => sp.ScheduleID == scheduleId && sp.IsActive)
                .Include(sp => sp.Page)
                    .ThenInclude(p => p.Layout)
                .OrderBy(sp => sp.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<SchedulePage>> GetSchedulesByPageAsync(int pageId)
        {
            return await _dbSet.AsNoTracking()
                .Where(sp => sp.PageID == pageId && sp.IsActive)
                .Include(sp => sp.Schedule)
                .OrderBy(sp => sp.Schedule.StartDate)
                .ToListAsync();
        }

        public async Task<SchedulePage?> GetByScheduleAndPageAsync(int scheduleId, int pageId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(sp => sp.ScheduleID == scheduleId && sp.PageID == pageId);
        }

        public async Task<int> GetMaxDisplayOrderAsync(int scheduleId)
        {
            var maxOrder = await _dbSet
                .Where(sp => sp.ScheduleID == scheduleId)
                .MaxAsync(sp => (int?)sp.DisplayOrder);

            return maxOrder ?? 0;
        }

        public async Task ReorderPagesAsync(int scheduleId, IEnumerable<int> schedulePageIds)
        {
            var pages = await _dbSet
                .Where(sp => sp.ScheduleID == scheduleId)
                .ToListAsync();

            var order = 1;
            foreach (var id in schedulePageIds)
            {
                var page = pages.FirstOrDefault(sp => sp.SchedulePageID == id);
                if (page != null)
                {
                    page.DisplayOrder = order++;
                }
            }
        }
    }
}
