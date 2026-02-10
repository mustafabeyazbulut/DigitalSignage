using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using DigitalSignage.Data;

namespace DigitalSignage.Services
{
    public class PageService : IPageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Page?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Pages.GetPageWithLayoutAsync(id);
        }

        public async Task<IEnumerable<Page>> GetAllAsync()
        {
            return await _unitOfWork.Pages.GetAllAsync();
        }

        public async Task<Page> CreateAsync(Page entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Pages.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task<Page> UpdateAsync(Page entity)
        {
            await _unitOfWork.Pages.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.Pages.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Page>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _unitOfWork.Pages.GetPagesByDepartmentAsync(departmentId);
        }

        public async Task<Page?> GetPageWithLayoutAsync(int pageId)
        {
            return await _unitOfWork.Pages.GetPageWithLayoutAsync(pageId);
        }

        public async Task<Page?> GetPageFullDetailsAsync(int pageId)
        {
            return await _unitOfWork.Pages.GetPageFullDetailsAsync(pageId);
        }

        public async Task<IEnumerable<Page>> GetActivePagesByDepartmentAsync(int departmentId)
        {
            return await _unitOfWork.Pages.GetActivePagesByDepartmentAsync(departmentId);
        }

        public async Task<PagedResult<Page>> GetPagesPagedAsync(
            int departmentId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            return await _unitOfWork.Pages.GetPagesPagedAsync(departmentId, pageNumber, pageSize, searchTerm, isActive);
        }
    }
}
