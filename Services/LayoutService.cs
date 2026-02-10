using DigitalSignage.Models;
using DigitalSignage.Data.Repositories;

namespace DigitalSignage.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly ILayoutRepository _layoutRepository;

        public LayoutService(ILayoutRepository layoutRepository)
        {
            _layoutRepository = layoutRepository;
        }

        public async Task<Layout?> GetByIdAsync(int id)
        {
            return await _layoutRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Layout>> GetAllAsync()
        {
            return await _layoutRepository.GetAllAsync();
        }

        public async Task<Layout> CreateAsync(Layout entity)
        {
            return await _layoutRepository.AddAsync(entity);
        }

        public async Task<Layout> UpdateAsync(Layout entity)
        {
            await _layoutRepository.UpdateAsync(entity);
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _layoutRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Layout>> GetByCompanyIdAsync(int companyId)
        {
            return await _layoutRepository.FindAsync(l => l.CompanyID == companyId);
        }
    }
}
