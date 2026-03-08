using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Services
{
    public interface IScheduleService : IService<Schedule>
    {
        Task<IEnumerable<Schedule>> GetByCompanyIdAsync(int companyId);
        Task<Schedule?> GetScheduleWithPagesAsync(int scheduleId);
        Task<IEnumerable<Schedule>> GetActiveSchedulesByCompanyAsync(int companyId);
        Task<PagedResult<Schedule>> GetSchedulesPagedAsync(int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task AddPageToScheduleAsync(int scheduleId, int pageId, int displayDuration, int displayOrder);
        Task RemovePageFromScheduleAsync(int schedulePageId);
        Task UpdateSchedulePageAsync(int schedulePageId, int displayDuration, int displayOrder);
        Task ReorderSchedulePagesAsync(int scheduleId, int[] schedulePageIds);
    }
}
