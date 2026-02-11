using DigitalSignage.Models;

namespace DigitalSignage.Services
{
    public interface ITenantContext
    {
        int CurrentCompanyId { get; }
        int CurrentUserId { get; }

        Task<bool> HasAccessToCompanyAsync(int companyId);
        Task<bool> IsCompanyAdminAsync(int companyId);
        Task<bool> IsDepartmentManagerAsync(int departmentId);
        Task<CompanyConfiguration?> GetCompanyConfigAsync(int companyId);
    }
}
