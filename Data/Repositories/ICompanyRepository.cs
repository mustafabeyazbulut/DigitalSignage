using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Data.Repositories
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<Company?> GetByCompanyCodeAsync(string companyCode);
        Task<Company?> GetCompanyWithDepartmentsAsync(int companyId);
        Task<Company?> GetCompanyWithLayoutsAsync(int companyId);
        Task<Company?> GetCompanyWithConfigurationAsync(int companyId);
        Task<Company?> GetCompanyFullDetailsAsync(int companyId);
        Task<IEnumerable<Company>> GetActiveCompaniesAsync();
        Task<PagedResult<Company>> GetCompaniesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<bool> IsCompanyCodeTakenAsync(string companyCode, int? excludeCompanyId = null);
    }
}
