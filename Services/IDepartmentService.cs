using DigitalSignage.Models;

namespace DigitalSignage.Services
{
    public interface IDepartmentService : IService<Department>
    {
        Task<IEnumerable<Department>> GetByCompanyIdAsync(int companyId);
    }
}
