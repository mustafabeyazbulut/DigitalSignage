using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using DigitalSignage.Data;

namespace DigitalSignage.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Company?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Companies.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            return await _unitOfWork.Companies.GetAllAsync();
        }

        public async Task<Company> CreateAsync(Company entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Companies.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task<Company> UpdateAsync(Company entity)
        {
            entity.ModifiedDate = DateTime.UtcNow;
            await _unitOfWork.Companies.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.Companies.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Company?> GetByCompanyCodeAsync(string companyCode)
        {
            return await _unitOfWork.Companies.GetByCompanyCodeAsync(companyCode);
        }

        public async Task<Company?> GetCompanyWithDepartmentsAsync(int companyId)
        {
            return await _unitOfWork.Companies.GetCompanyWithDepartmentsAsync(companyId);
        }

        public async Task<Company?> GetCompanyWithConfigurationAsync(int companyId)
        {
            return await _unitOfWork.Companies.GetCompanyWithConfigurationAsync(companyId);
        }

        public async Task<IEnumerable<Company>> GetActiveCompaniesAsync()
        {
            return await _unitOfWork.Companies.GetActiveCompaniesAsync();
        }

        public async Task<PagedResult<Company>> GetCompaniesPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            return await _unitOfWork.Companies.GetCompaniesPagedAsync(pageNumber, pageSize, searchTerm, isActive);
        }
    }
}
