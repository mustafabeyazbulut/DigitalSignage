using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using DigitalSignage.Data;
using Microsoft.EntityFrameworkCore;

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
            entity.PageCode = await GeneratePageCodeAsync();
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

        public async Task<string> GeneratePageCodeAsync()
        {
            var lastPage = await _unitOfWork.Pages.QueryAsNoTracking()
                .Where(p => p.PageCode.StartsWith("PG-"))
                .OrderByDescending(p => p.PageCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastPage != null && lastPage.PageCode.Length > 3)
            {
                var numPart = lastPage.PageCode.Substring(3);
                if (int.TryParse(numPart, out int parsed))
                    nextNumber = parsed + 1;
            }

            return $"PG-{nextNumber:D5}";
        }

        public async Task<bool> AssignLayoutAsync(int pageId, int layoutId)
        {
            var page = await _unitOfWork.Pages.GetByIdAsync(pageId);
            if (page == null)
                return false;

            page.LayoutID = layoutId;
            await _unitOfWork.Pages.UpdateAsync(page);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
