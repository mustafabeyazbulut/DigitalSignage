using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Services
{
    public interface IDepartmentService : IService<Department>
    {
        Task<IEnumerable<Department>> GetByCompanyIdAsync(int companyId);
        Task<Department?> GetWithCompanyAsync(int departmentId);
        Task<Department?> GetDepartmentWithPagesAsync(int departmentId);
        Task<IEnumerable<Department>> GetActiveDepartmentsByCompanyAsync(int companyId);
        Task<PagedResult<Department>> GetDepartmentsPagedAsync(int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
    }
}
