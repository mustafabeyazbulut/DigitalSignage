using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Data.Repositories
{
    public interface ILayoutRepository : IRepository<Layout>
    {
        Task<IEnumerable<Layout>> GetLayoutsByCompanyAsync(int companyId);
        Task<Layout?> GetLayoutWithSectionsAsync(int layoutId);
        Task<Layout?> GetLayoutWithPagesAsync(int layoutId);
        Task<IEnumerable<Layout>> GetActiveLayoutsByCompanyAsync(int companyId);
        Task<PagedResult<Layout>> GetLayoutsPagedAsync(int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
    }
}
