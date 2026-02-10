using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public interface ICompanyConfigurationRepository : IRepository<CompanyConfiguration>
    {
        Task<CompanyConfiguration?> GetByCompanyAsync(int companyId);
        Task<CompanyConfiguration> GetOrCreateByCompanyAsync(int companyId);
    }
}
