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

        public async Task<bool> AssignLayoutAsync(int pageId, int layoutId)
        {
            var page = await _unitOfWork.Pages.GetByIdAsync(pageId);
            if (page == null)
                return false;

            // Layout değişiyorsa eski section bazlı içerik atamalarını temizle
            if (page.LayoutID != layoutId)
            {
                var sectionContents = await _unitOfWork.PageContents
                    .Query()
                    .Where(pc => pc.PageID == pageId && pc.DisplaySection != null && pc.DisplaySection != "")
                    .ToListAsync();

                foreach (var pc in sectionContents)
                {
                    await _unitOfWork.PageContents.DeleteAsync(pc.PageContentID);
                }
            }

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

            // Aynı içerik aynı section'a zaten atanmış mı kontrol et
            var sectionContents = await _unitOfWork.PageContents.GetContentsBySectionAsync(pageId, sectionPosition);
            if (sectionContents.Any(pc => pc.ContentID == contentId))
                return false; // Zaten atanmış

            // Yeni atama oluştur (çoklu içerik destekli)
            var maxOrder = await _unitOfWork.PageContents.GetMaxSectionDisplayOrderAsync(pageId, sectionPosition);
            var pageContent = new PageContent
            {
                PageID = pageId,
                ContentID = contentId,
                DisplaySection = sectionPosition,
                DisplayOrder = maxOrder + 1,
                DurationSeconds = 10,
                IsActive = true,
                AddedDate = DateTime.UtcNow
            };
            await _unitOfWork.PageContents.AddAsync(pageContent);

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveContentFromSectionAsync(int pageId, string sectionPosition)
        {
            var sectionContents = await _unitOfWork.PageContents
                .Query()
                .Where(pc => pc.PageID == pageId && pc.DisplaySection == sectionPosition && pc.IsActive)
                .ToListAsync();

            if (!sectionContents.Any()) return false;

            foreach (var pc in sectionContents)
            {
                await _unitOfWork.PageContents.DeleteAsync(pc.PageContentID);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveSingleContentFromSectionAsync(int pageId, string sectionPosition, int pageContentId)
        {
            var pc = await _unitOfWork.PageContents.GetByIdAsync(pageContentId);
            if (pc == null || pc.PageID != pageId || pc.DisplaySection != sectionPosition)
                return false;

            await _unitOfWork.PageContents.DeleteAsync(pageContentId);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateSectionContentDurationAsync(int pageContentId, int durationSeconds)
        {
            if (durationSeconds < 1) durationSeconds = 1;

            var pc = await _unitOfWork.PageContents.GetByIdAsync(pageContentId);
            if (pc == null) return false;

            pc.DurationSeconds = durationSeconds;
            await _unitOfWork.PageContents.UpdateAsync(pc);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReorderSectionContentsAsync(int pageId, string sectionPosition, int[] orderedIds)
        {
            var sectionContents = await _unitOfWork.PageContents
                .Query()
                .Where(pc => pc.PageID == pageId && pc.DisplaySection == sectionPosition && pc.IsActive)
                .ToListAsync();

            var order = 1;
            foreach (var id in orderedIds)
            {
                var pc = sectionContents.FirstOrDefault(c => c.PageContentID == id);
                if (pc != null)
                {
                    pc.DisplayOrder = order++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<string, List<PageContent>>> GetSectionContentMapAsync(int pageId)
        {
            var contents = await _unitOfWork.PageContents.GetContentsByPageAsync(pageId);
            return contents
                .Where(pc => !string.IsNullOrEmpty(pc.DisplaySection))
                .GroupBy(pc => pc.DisplaySection!)
                .ToDictionary(g => g.Key, g => g.OrderBy(pc => pc.DisplayOrder).ToList());
        }
    }
}
