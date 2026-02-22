using DigitalSignage.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class PageContentRepository : Repository<PageContent>, IPageContentRepository
    {
        public PageContentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<PageContent>> GetContentsByPageAsync(int pageId)
        {
            return await _dbSet.AsNoTracking()
                .Where(pc => pc.PageID == pageId && pc.IsActive)
                .Include(pc => pc.Content)
                .OrderBy(pc => pc.DisplayOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<PageContent>> GetPagesByContentAsync(int contentId)
        {
            return await _dbSet.AsNoTracking()
                .Where(pc => pc.ContentID == contentId && pc.IsActive)
                .Include(pc => pc.Page)
                .OrderBy(pc => pc.DisplayOrder)
                .ToListAsync();
        }

        public async Task<PageContent?> GetByPageAndContentAsync(int pageId, int contentId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(pc => pc.PageID == pageId && pc.ContentID == contentId);
        }

        public async Task<int> GetMaxDisplayOrderAsync(int pageId)
        {
            var maxOrder = await _dbSet
                .Where(pc => pc.PageID == pageId)
                .MaxAsync(pc => (int?)pc.DisplayOrder);

            return maxOrder ?? 0;
        }

        public async Task<PageContent?> GetByPageAndSectionAsync(int pageId, string sectionPosition)
        {
            return await _dbSet
                .Include(pc => pc.Content)
                .FirstOrDefaultAsync(pc => pc.PageID == pageId && pc.DisplaySection == sectionPosition && pc.IsActive);
        }

        public async Task ReorderContentsAsync(int pageId, IEnumerable<int> pageContentIds)
        {
            var contents = await _dbSet
                .Where(pc => pc.PageID == pageId)
                .ToListAsync();

            var order = 1;
            foreach (var id in pageContentIds)
            {
                var content = contents.FirstOrDefault(pc => pc.PageContentID == id);
                if (content != null)
                {
                    content.DisplayOrder = order++;
                }
            }
        }
    }
}
