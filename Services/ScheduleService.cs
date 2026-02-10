using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using DigitalSignage.Data;

namespace DigitalSignage.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScheduleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Schedule?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Schedules.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Schedule>> GetAllAsync()
        {
            return await _unitOfWork.Schedules.GetAllAsync();
        }

        public async Task<Schedule> CreateAsync(Schedule entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Schedules.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task<Schedule> UpdateAsync(Schedule entity)
        {
            await _unitOfWork.Schedules.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.Schedules.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Schedule>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _unitOfWork.Schedules.GetSchedulesByDepartmentAsync(departmentId);
        }

        public async Task<Schedule?> GetScheduleWithPagesAsync(int scheduleId)
        {
            return await _unitOfWork.Schedules.GetScheduleWithPagesAsync(scheduleId);
        }

        public async Task<IEnumerable<Schedule>> GetActiveSchedulesByDepartmentAsync(int departmentId)
        {
            return await _unitOfWork.Schedules.GetActiveSchedulesByDepartmentAsync(departmentId);
        }

        public async Task<IEnumerable<Schedule>> GetCurrentActiveSchedulesAsync(int departmentId)
        {
            return await _unitOfWork.Schedules.GetCurrentActiveSchedulesAsync(departmentId);
        }

        public async Task<PagedResult<Schedule>> GetSchedulesPagedAsync(
            int departmentId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            return await _unitOfWork.Schedules.GetSchedulesPagedAsync(departmentId, pageNumber, pageSize, searchTerm, isActive);
        }
    }
}
