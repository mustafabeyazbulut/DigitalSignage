using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Services
{
    public interface ILayoutService : IService<Layout>
    {
        Task<IEnumerable<Layout>> GetByCompanyIdAsync(int companyId);
        Task<Layout?> GetLayoutWithSectionsAsync(int layoutId);
        Task<IEnumerable<Layout>> GetActiveLayoutsByCompanyAsync(int companyId);
        Task<PagedResult<Layout>> GetLayoutsPagedAsync(int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
    }
}
