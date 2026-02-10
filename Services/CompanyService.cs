using DigitalSignage.Models;
using DigitalSignage.Data.Repositories;

namespace DigitalSignage.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<Company?> GetByIdAsync(int id)
        {
            return await _companyRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            return await _companyRepository.GetAllAsync();
        }

        public async Task<Company> CreateAsync(Company entity)
        {
            return await _companyRepository.AddAsync(entity);
        }

        public async Task<Company> UpdateAsync(Company entity)
        {
            entity.ModifiedDate = DateTime.UtcNow;
            await _companyRepository.UpdateAsync(entity);
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _companyRepository.DeleteAsync(id);
        }

        public async Task<Company?> GetByCompanyCodeAsync(string companyCode)
        {
            return await _companyRepository.FirstOrDefaultAsync(c => c.CompanyCode == companyCode);
        }
    }
}
