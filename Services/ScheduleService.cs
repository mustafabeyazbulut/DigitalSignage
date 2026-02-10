using DigitalSignage.Models;
using DigitalSignage.Data.Repositories;

namespace DigitalSignage.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;

        public ScheduleService(IScheduleRepository scheduleRepository)
        {
            _scheduleRepository = scheduleRepository;
        }

        public async Task<Schedule?> GetByIdAsync(int id)
        {
            return await _scheduleRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Schedule>> GetAllAsync()
        {
            return await _scheduleRepository.GetAllAsync();
        }

        public async Task<Schedule> CreateAsync(Schedule entity)
        {
            return await _scheduleRepository.AddAsync(entity);
        }

        public async Task<Schedule> UpdateAsync(Schedule entity)
        {
            await _scheduleRepository.UpdateAsync(entity);
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _scheduleRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Schedule>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _scheduleRepository.FindAsync(s => s.DepartmentID == departmentId);
        }

        public async Task<IEnumerable<Schedule>> GetActiveSchedulesAsync()
        {
            var now = DateTime.UtcNow;
            return await _scheduleRepository.FindAsync(s => s.IsActive && s.StartDate <= now && s.EndDate >= now);
        }
    }
}
