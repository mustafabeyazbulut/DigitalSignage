using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public interface ISchedulePageRepository : IRepository<SchedulePage>
    {
        Task<IEnumerable<SchedulePage>> GetPagesByScheduleAsync(int scheduleId);
        Task<IEnumerable<SchedulePage>> GetSchedulesByPageAsync(int pageId);
        Task<SchedulePage?> GetByScheduleAndPageAsync(int scheduleId, int pageId);
        Task<int> GetMaxDisplayOrderAsync(int scheduleId);
        Task ReorderPagesAsync(int scheduleId, IEnumerable<int> schedulePageIds);
    }
}
