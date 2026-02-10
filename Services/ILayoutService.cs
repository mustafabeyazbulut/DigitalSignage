using DigitalSignage.Models;

namespace DigitalSignage.Services
{
    public interface ILayoutService : IService<Layout>
    {
        Task<IEnumerable<Layout>> GetByCompanyIdAsync(int companyId);
    }
}
