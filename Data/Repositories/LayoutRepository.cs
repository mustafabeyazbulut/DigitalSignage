using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class LayoutRepository : Repository<Layout>, ILayoutRepository
    {
        public LayoutRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Layout>> GetLayoutsByCompanyAsync(int companyId)
        {
            return await _dbSet.AsNoTracking()
                .Where(l => l.CompanyID == companyId)
                .OrderBy(l => l.LayoutName)
                .ToListAsync();
        }

        public async Task<Layout?> GetLayoutWithSectionsAsync(int layoutId)
        {
            return await _dbSet
                .Include(l => l.LayoutSections.OrderBy(ls => ls.RowIndex).ThenBy(ls => ls.ColumnIndex))
                .FirstOrDefaultAsync(l => l.LayoutID == layoutId);
        }

        public async Task<Layout?> GetLayoutWithPagesAsync(int layoutId)
        {
            return await _dbSet
                .Include(l => l.Pages.Where(p => p.IsActive))
                .FirstOrDefaultAsync(l => l.LayoutID == layoutId);
        }

        public async Task<IEnumerable<Layout>> GetActiveLayoutsByCompanyAsync(int companyId)
        {
            return await _dbSet.AsNoTracking()
                .Where(l => l.CompanyID == companyId && l.IsActive)
                .OrderBy(l => l.LayoutName)
                .ToListAsync();
        }

        public async Task<PagedResult<Layout>> GetLayoutsPagedAsync(
            int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            IQueryable<Layout> query = _dbSet.AsNoTracking()
                .Where(l => l.CompanyID == companyId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(l =>
                    l.LayoutName.ToLower().Contains(term) ||
                    l.LayoutCode.ToLower().Contains(term) ||
                    (l.Description != null && l.Description.ToLower().Contains(term)));
            }

            if (isActive.HasValue)
                query = query.Where(l => l.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(l => l.LayoutName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Layout>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
