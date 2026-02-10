using DigitalSignage.Models;

namespace DigitalSignage.Services
{
    public interface IScheduleService : IService<Schedule>
    {
        Task<IEnumerable<Schedule>> GetByDepartmentIdAsync(int departmentId);
        Task<IEnumerable<Schedule>> GetActiveSchedulesAsync();
    }
}
