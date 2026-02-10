using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using DigitalSignage.Data;

namespace DigitalSignage.Services
{
    public class ContentService : IContentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Content?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Contents.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Content>> GetAllAsync()
        {
            return await _unitOfWork.Contents.GetAllAsync();
        }

        public async Task<Content> CreateAsync(Content entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Contents.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task<Content> UpdateAsync(Content entity)
        {
            entity.ModifiedDate = DateTime.UtcNow;
            await _unitOfWork.Contents.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.Contents.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Content>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _unitOfWork.Contents.GetContentsByDepartmentAsync(departmentId);
        }

        public async Task<IEnumerable<Content>> GetContentsByTypeAsync(int departmentId, string contentType)
        {
            return await _unitOfWork.Contents.GetContentsByTypeAsync(departmentId, contentType);
        }

        public async Task<IEnumerable<Content>> GetActiveContentsByDepartmentAsync(int departmentId)
        {
            return await _unitOfWork.Contents.GetActiveContentsByDepartmentAsync(departmentId);
        }

        public async Task<PagedResult<Content>> GetContentsPagedAsync(
            int departmentId, int pageNumber, int pageSize,
            string? contentType = null, string? searchTerm = null, bool? isActive = null)
        {
            return await _unitOfWork.Contents.GetContentsPagedAsync(departmentId, pageNumber, pageSize, contentType, searchTerm, isActive);
        }
    }
}
