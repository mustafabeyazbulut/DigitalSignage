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
        Task<bool> AssignLayoutAsync(int pageId, int layoutId);
        Task<bool> AssignContentToSectionAsync(int pageId, string sectionPosition, int contentId);
        Task<bool> RemoveContentFromSectionAsync(int pageId, string sectionPosition);
        Task<bool> RemoveSingleContentFromSectionAsync(int pageId, string sectionPosition, int pageContentId);
        Task<bool> UpdateSectionContentDurationAsync(int pageContentId, int durationSeconds);
        Task<bool> ReorderSectionContentsAsync(int pageId, string sectionPosition, int[] orderedIds);
        Task<Dictionary<string, List<PageContent>>> GetSectionContentMapAsync(int pageId);
    }
}
