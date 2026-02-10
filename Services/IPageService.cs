using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Services
{
    public interface IPageService : IService<Page>
    {
        Task<IEnumerable<Page>> GetByDepartmentIdAsync(int departmentId);
        Task<Page?> GetPageWithLayoutAsync(int pageId);
        Task<Page?> GetPageFullDetailsAsync(int pageId);
        Task<IEnumerable<Page>> GetActivePagesByDepartmentAsync(int departmentId);
        Task<PagedResult<Page>> GetPagesPagedAsync(int departmentId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
    }
}
