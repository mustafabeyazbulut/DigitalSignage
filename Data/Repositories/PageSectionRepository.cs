using DigitalSignage.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class PageSectionRepository : Repository<PageSection>, IPageSectionRepository
    {
        public PageSectionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<PageSection>> GetSectionsByPageAsync(int pageId)
        {
            return await _dbSet.AsNoTracking()
                .Where(ps => ps.PageID == pageId && ps.IsActive)
                .Include(ps => ps.LayoutSection)
                .OrderBy(ps => ps.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PageSection?> GetByPageAndLayoutSectionAsync(int pageId, int layoutSectionId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ps => ps.PageID == pageId && ps.LayoutSectionID == layoutSectionId);
        }

        public async Task DeleteSectionsByPageAsync(int pageId)
        {
            var sections = await _dbSet
                .Where(ps => ps.PageID == pageId)
                .ToListAsync();

            _dbSet.RemoveRange(sections);
        }
    }
}
