using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public interface IPageContentRepository : IRepository<PageContent>
    {
        Task<IEnumerable<PageContent>> GetContentsByPageAsync(int pageId);
        Task<IEnumerable<PageContent>> GetPagesByContentAsync(int contentId);
        Task<PageContent?> GetByPageAndContentAsync(int pageId, int contentId);
        Task<int> GetMaxDisplayOrderAsync(int pageId);
        Task ReorderContentsAsync(int pageId, IEnumerable<int> pageContentIds);
        Task<List<PageContent>> GetContentsBySectionAsync(int pageId, string sectionPosition);
        Task<int> GetMaxSectionDisplayOrderAsync(int pageId, string sectionPosition);
    }
}
