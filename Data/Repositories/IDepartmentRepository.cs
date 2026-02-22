using DigitalSignage.Models;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Data.Repositories
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        Task<IEnumerable<Department>> GetDepartmentsByCompanyAsync(int companyId);
        Task<Department?> GetWithCompanyAsync(int departmentId);
        Task<Department?> GetDepartmentWithPagesAsync(int departmentId);
        Task<Department?> GetDepartmentWithSchedulesAsync(int departmentId);
        Task<Department?> GetDepartmentWithContentsAsync(int departmentId);
        Task<Department?> GetDepartmentFullDetailsAsync(int departmentId);
        Task<IEnumerable<Department>> GetActiveDepartmentsByCompanyAsync(int companyId);
        Task<PagedResult<Department>> GetDepartmentsPagedAsync(int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
    }
}
