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

        public async Task<bool> AssignContentToSectionAsync(int pageId, string sectionPosition, int contentId)
        {
            var page = await _unitOfWork.Pages.GetByIdAsync(pageId);
            if (page == null) return false;

            var content = await _unitOfWork.Contents.GetByIdAsync(contentId);
            if (content == null) return false;

            // Aynı section'da mevcut atama var mı kontrol et
            var existing = await _unitOfWork.PageContents.GetByPageAndSectionAsync(pageId, sectionPosition);
            if (existing != null)
            {
                // Mevcut atamayı güncelle
                existing.ContentID = contentId;
                existing.AddedDate = DateTime.UtcNow;
                await _unitOfWork.PageContents.UpdateAsync(existing);
            }
            else
            {
                // Yeni atama oluştur
                var maxOrder = await _unitOfWork.PageContents.GetMaxDisplayOrderAsync(pageId);
                var pageContent = new PageContent
                {
                    PageID = pageId,
                    ContentID = contentId,
                    DisplaySection = sectionPosition,
                    DisplayOrder = maxOrder + 1,
                    IsActive = true,
                    AddedDate = DateTime.UtcNow
                };
                await _unitOfWork.PageContents.AddAsync(pageContent);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveContentFromSectionAsync(int pageId, string sectionPosition)
        {
            var existing = await _unitOfWork.PageContents.GetByPageAndSectionAsync(pageId, sectionPosition);
            if (existing == null) return false;

            await _unitOfWork.PageContents.DeleteAsync(existing.PageContentID);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<string, PageContent>> GetSectionContentMapAsync(int pageId)
        {
            var contents = await _unitOfWork.PageContents.GetContentsByPageAsync(pageId);
            return contents
                .Where(pc => !string.IsNullOrEmpty(pc.DisplaySection))
                .GroupBy(pc => pc.DisplaySection!)
                .ToDictionary(g => g.Key, g => g.First());
        }
    }
}
