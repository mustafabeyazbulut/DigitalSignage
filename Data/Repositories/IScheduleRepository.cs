using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Data.Repositories
{
    public interface IScheduleRepository : IRepository<Schedule>
    {
        Task<IEnumerable<Schedule>> GetSchedulesByCompanyAsync(int companyId);
        Task<Schedule?> GetScheduleWithPagesAsync(int scheduleId);
        Task<IEnumerable<Schedule>> GetActiveSchedulesByCompanyAsync(int companyId);
        Task<PagedResult<Schedule>> GetSchedulesPagedAsync(int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
    }
}
