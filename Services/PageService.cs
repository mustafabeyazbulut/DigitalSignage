using DigitalSignage.Models;
using DigitalSignage.Data.Repositories;

namespace DigitalSignage.Services
{
    public class PageService : IPageService
    {
        private readonly IPageRepository _pageRepository;

        public PageService(IPageRepository pageRepository)
        {
            _pageRepository = pageRepository;
        }

        public async Task<Page?> GetByIdAsync(int id)
        {
            // Include Layout and Content?
            return await _pageRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Page>> GetAllAsync()
        {
            return await _pageRepository.GetAllAsync();
        }

        public async Task<Page> CreateAsync(Page entity)
        {
            return await _pageRepository.AddAsync(entity);
        }

        public async Task<Page> UpdateAsync(Page entity)
        {
            await _pageRepository.UpdateAsync(entity);
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _pageRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Page>> GetByDepartmentIdAsync(int departmentId)
        {
            return await _pageRepository.FindAsync(p => p.DepartmentID == departmentId);
        }
    }
}
