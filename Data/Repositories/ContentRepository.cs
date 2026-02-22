using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class ContentRepository : Repository<Content>, IContentRepository
    {
        public ContentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Content>> GetContentsByDepartmentAsync(int departmentId)
        {
            return await _dbSet.AsNoTracking()
                .Where(c => c.DepartmentID == departmentId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Content>> GetContentsByTypeAsync(int departmentId, string contentType)
        {
            return await _dbSet.AsNoTracking()
                .Where(c => c.DepartmentID == departmentId && c.ContentType == contentType)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<Content?> GetContentWithDepartmentAsync(int contentId)
        {
            return await _dbSet
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.ContentID == contentId);
        }

        public async Task<Content?> GetContentWithPagesAsync(int contentId)
        {
            return await _dbSet
                .Include(c => c.PageContents)
                    .ThenInclude(pc => pc.Page)
                .FirstOrDefaultAsync(c => c.ContentID == contentId);
        }

        public async Task<IEnumerable<Content>> GetActiveContentsByDepartmentAsync(int departmentId)
        {
            return await _dbSet.AsNoTracking()
                .Where(c => c.DepartmentID == departmentId && c.IsActive)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Content>> GetContentsPagedAsync(
            int departmentId, int pageNumber, int pageSize,
            string? contentType = null, string? searchTerm = null, bool? isActive = null)
        {
            IQueryable<Content> query = _dbSet.AsNoTracking()
                .Where(c => c.DepartmentID == departmentId);

            if (!string.IsNullOrWhiteSpace(contentType))
                query = query.Where(c => c.ContentType == contentType);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(c =>
                    (c.ContentTitle != null && c.ContentTitle.ToLower().Contains(term)) ||
                    (c.ContentData != null && c.ContentData.ToLower().Contains(term)));
            }

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Content>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
