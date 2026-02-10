using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Services
{
    public interface IContentService : IService<Content>
    {
        Task<IEnumerable<Content>> GetByDepartmentIdAsync(int departmentId);
        Task<IEnumerable<Content>> GetContentsByTypeAsync(int departmentId, string contentType);
        Task<IEnumerable<Content>> GetActiveContentsByDepartmentAsync(int departmentId);
        Task<PagedResult<Content>> GetContentsPagedAsync(int departmentId, int pageNumber, int pageSize, string? contentType = null, string? searchTerm = null, bool? isActive = null);
    }
}
