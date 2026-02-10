using DigitalSignage.Models;

namespace DigitalSignage.Services
{
    public interface ICompanyService : IService<Company>
    {
        Task<Company?> GetByCompanyCodeAsync(string companyCode);
    }
}
