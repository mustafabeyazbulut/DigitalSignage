using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Services
{
    public interface IScheduleService : IService<Schedule>
    {
        Task<IEnumerable<Schedule>> GetByDepartmentIdAsync(int departmentId);
        Task<Schedule?> GetScheduleWithPagesAsync(int scheduleId);
        Task<IEnumerable<Schedule>> GetActiveSchedulesByDepartmentAsync(int departmentId);
        Task<IEnumerable<Schedule>> GetCurrentActiveSchedulesAsync(int departmentId);
        Task<PagedResult<Schedule>> GetSchedulesPagedAsync(int departmentId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
    }
}
