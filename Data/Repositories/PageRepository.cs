using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class PageRepository : Repository<Page>, IPageRepository
    {
        public PageRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Page>> GetPagesByDepartmentAsync(int departmentId)
        {
            return await _dbSet.AsNoTracking()
                .Where(p => p.DepartmentID == departmentId)
                .Include(p => p.Layout)
                .OrderBy(p => p.PageName)
                .ToListAsync();
        }

        public async Task<Page?> GetPageWithLayoutAsync(int pageId)
        {
            return await _dbSet
                .Include(p => p.Layout)
                    .ThenInclude(l => l.LayoutSections)
                .FirstOrDefaultAsync(p => p.PageID == pageId);
        }

        public async Task<Page?> GetPageWithContentsAsync(int pageId)
        {
            return await _dbSet
                .Include(p => p.PageContents.Where(pc => pc.IsActive))
                    .ThenInclude(pc => pc.Content)
                .FirstOrDefaultAsync(p => p.PageID == pageId);
        }

        public async Task<Page?> GetPageWithSectionsAsync(int pageId)
        {
            return await _dbSet
                .Include(p => p.PageSections.Where(ps => ps.IsActive))
                    .ThenInclude(ps => ps.LayoutSection)
                .FirstOrDefaultAsync(p => p.PageID == pageId);
        }

        public async Task<Page?> GetPageFullDetailsAsync(int pageId)
        {
            return await _dbSet
                .Include(p => p.Department)
                .Include(p => p.Layout)
                    .ThenInclude(l => l.LayoutSections)
                .Include(p => p.PageContents.OrderBy(pc => pc.DisplayOrder))
                    .ThenInclude(pc => pc.Content)
                .Include(p => p.PageSections.OrderBy(ps => ps.DisplayOrder))
                    .ThenInclude(ps => ps.LayoutSection)
                .Include(p => p.SchedulePages)
                    .ThenInclude(sp => sp.Schedule)
                .FirstOrDefaultAsync(p => p.PageID == pageId);
        }

        public async Task<IEnumerable<Page>> GetActivePagesByDepartmentAsync(int departmentId)
        {
            return await _dbSet.AsNoTracking()
                .Where(p => p.DepartmentID == departmentId && p.IsActive)
                .Include(p => p.Layout)
                .OrderBy(p => p.PageName)
                .ToListAsync();
        }

        public async Task<PagedResult<Page>> GetPagesPagedAsync(
            int departmentId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            IQueryable<Page> query = _dbSet.AsNoTracking()
                .Where(p => p.DepartmentID == departmentId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p =>
                    p.PageName.ToLower().Contains(term) ||
                    p.PageTitle.ToLower().Contains(term) ||
                    p.PageCode.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)));
            }

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .Include(p => p.Layout)
                .OrderBy(p => p.PageName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Page>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
