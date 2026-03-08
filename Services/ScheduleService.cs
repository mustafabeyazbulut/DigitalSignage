using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using DigitalSignage.Data;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<Schedule>> GetByCompanyIdAsync(int companyId)
        {
            return await _unitOfWork.Schedules.GetSchedulesByCompanyAsync(companyId);
        }

        public async Task<Schedule?> GetScheduleWithPagesAsync(int scheduleId)
        {
            return await _unitOfWork.Schedules.GetScheduleWithPagesAsync(scheduleId);
        }

        public async Task<IEnumerable<Schedule>> GetActiveSchedulesByCompanyAsync(int companyId)
        {
            return await _unitOfWork.Schedules.GetActiveSchedulesByCompanyAsync(companyId);
        }

        public async Task<PagedResult<Schedule>> GetSchedulesPagedAsync(
            int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            return await _unitOfWork.Schedules.GetSchedulesPagedAsync(companyId, pageNumber, pageSize, searchTerm, isActive);
        }

        public async Task AddPageToScheduleAsync(int scheduleId, int pageId, int displayDuration, int displayOrder)
        {
            var existing = await _unitOfWork.SchedulePages.Query()
                .FirstOrDefaultAsync(sp => sp.ScheduleID == scheduleId && sp.PageID == pageId);

            if (existing != null)
                return;

            var schedulePage = new SchedulePage
            {
                ScheduleID = scheduleId,
                PageID = pageId,
                DisplayDuration = displayDuration,
                DisplayOrder = displayOrder,
                IsActive = true
            };

            await _unitOfWork.SchedulePages.AddAsync(schedulePage);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemovePageFromScheduleAsync(int schedulePageId)
        {
            await _unitOfWork.SchedulePages.DeleteAsync(schedulePageId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateSchedulePageAsync(int schedulePageId, int displayDuration, int displayOrder)
        {
            var sp = await _unitOfWork.SchedulePages.GetByIdAsync(schedulePageId);
            if (sp == null) return;

            sp.DisplayDuration = displayDuration;
            sp.DisplayOrder = displayOrder;
            await _unitOfWork.SchedulePages.UpdateAsync(sp);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ReorderSchedulePagesAsync(int scheduleId, int[] schedulePageIds)
        {
            var schedulePages = await _unitOfWork.SchedulePages.Query()
                .Where(sp => sp.ScheduleID == scheduleId)
                .ToListAsync();

            for (int i = 0; i < schedulePageIds.Length; i++)
            {
                var sp = schedulePages.FirstOrDefault(x => x.SchedulePageID == schedulePageIds[i]);
                if (sp != null)
                {
                    sp.DisplayOrder = i + 1;
                    await _unitOfWork.SchedulePages.UpdateAsync(sp);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
