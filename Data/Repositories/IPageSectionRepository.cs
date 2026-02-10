using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public interface IPageSectionRepository : IRepository<PageSection>
    {
        Task<IEnumerable<PageSection>> GetSectionsByPageAsync(int pageId);
        Task<PageSection?> GetByPageAndLayoutSectionAsync(int pageId, int layoutSectionId);
        Task DeleteSectionsByPageAsync(int pageId);
    }
}
