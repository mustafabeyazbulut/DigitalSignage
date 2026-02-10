using DigitalSignage.Models;
using DigitalSignage.Data.Repositories;

namespace DigitalSignage.Services
{
    public class ContentService : IContentService
    {
        private readonly IContentRepository _contentRepository;

        public ContentService(IContentRepository contentRepository)
        {
            _contentRepository = contentRepository;
        }

        public async Task<Content?> GetByIdAsync(int id)
        {
            return await _contentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Content>> GetAllAsync()
        {
            return await _contentRepository.GetAllAsync();
        }

        public async Task<Content> CreateAsync(Content entity)
        {
            return await _contentRepository.AddAsync(entity);
        }

        public async Task<Content> UpdateAsync(Content entity)
        {
            entity.ModifiedDate = DateTime.UtcNow;
            await _contentRepository.UpdateAsync(entity);
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _contentRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Content>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _contentRepository.FindAsync(c => c.DepartmentID == departmentId);
        }
    }
}
