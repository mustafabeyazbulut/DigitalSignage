using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Services
{
    public interface ICompanyService : IService<Company>
    {
        Task<Company?> GetByCompanyCodeAsync(string companyCode);
        Task<Company?> GetCompanyWithDepartmentsAsync(int companyId);
        Task<Company?> GetCompanyWithConfigurationAsync(int companyId);
        Task<IEnumerable<Company>> GetActiveCompaniesAsync();
        Task<PagedResult<Company>> GetCompaniesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
    }
}
