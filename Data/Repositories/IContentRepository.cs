using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Data.Repositories
{
    public interface IContentRepository : IRepository<Content>
    {
        Task<IEnumerable<Content>> GetContentsByDepartmentAsync(int departmentId);
        Task<IEnumerable<Content>> GetContentsByTypeAsync(int departmentId, string contentType);
        Task<Content?> GetContentWithDepartmentAsync(int contentId);
        Task<Content?> GetContentWithPagesAsync(int contentId);
        Task<IEnumerable<Content>> GetActiveContentsByDepartmentAsync(int departmentId);
        Task<PagedResult<Content>> GetContentsPagedAsync(int departmentId, int pageNumber, int pageSize, string? contentType = null, string? searchTerm = null, bool? isActive = null);
    }
}
