using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Data.Repositories
{
    public interface IScheduleRepository : IRepository<Schedule>
    {
        Task<IEnumerable<Schedule>> GetSchedulesByDepartmentAsync(int departmentId);
        Task<Schedule?> GetScheduleWithPagesAsync(int scheduleId);
        Task<IEnumerable<Schedule>> GetActiveSchedulesByDepartmentAsync(int departmentId);
        Task<IEnumerable<Schedule>> GetSchedulesByDateRangeAsync(int departmentId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<Schedule>> GetCurrentActiveSchedulesAsync(int departmentId);
        Task<PagedResult<Schedule>> GetSchedulesPagedAsync(int departmentId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
    }
}
