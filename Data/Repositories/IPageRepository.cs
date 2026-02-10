using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Data.Repositories
{
    public interface IPageRepository : IRepository<Page>
    {
        Task<IEnumerable<Page>> GetPagesByDepartmentAsync(int departmentId);
        Task<Page?> GetPageWithLayoutAsync(int pageId);
        Task<Page?> GetPageWithContentsAsync(int pageId);
        Task<Page?> GetPageWithSectionsAsync(int pageId);
        Task<Page?> GetPageFullDetailsAsync(int pageId);
        Task<IEnumerable<Page>> GetActivePagesByDepartmentAsync(int departmentId);
        Task<PagedResult<Page>> GetPagesPagedAsync(int departmentId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
    }
}
