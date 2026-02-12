using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using DigitalSignage.Data;
using DigitalSignage.Models;

namespace DigitalSignage.Services
{
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;

        public TenantContext(
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork,
            IMemoryCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _cache = cache;
        }

        public int CurrentCompanyId
        {
            get
            {
                if (_httpContextAccessor.HttpContext?.Items["CompanyId"] is int companyId)
                    return companyId;

                var companyIdSession = _httpContextAccessor.HttpContext?.Session.GetInt32("SelectedCompanyId");
                return companyIdSession ?? 0;
            }
        }

        public int CurrentUserId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
                return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
            }
        }

        public async Task<bool> HasAccessToCompanyAsync(int companyId)
        {
            if (CurrentUserId == 0)
                return false;

            var role = await _unitOfWork.UserCompanyRoles.FirstOrDefaultAsync(ucr =>
                ucr.UserID == CurrentUserId &&
                ucr.CompanyID == companyId &&
                ucr.IsActive
            );

            return role != null;
        }

        public async Task<bool> IsCompanyAdminAsync(int companyId)
        {
            if (CurrentUserId == 0)
                return false;

            var role = await _unitOfWork.UserCompanyRoles.FirstOrDefaultAsync(ucr =>
                ucr.UserID == CurrentUserId &&
                ucr.CompanyID == companyId &&
                ucr.Role == "CompanyAdmin" &&
                ucr.IsActive
            );

            return role != null;
        }

        public async Task<bool> IsDepartmentManagerAsync(int departmentId)
        {
            if (CurrentUserId == 0)
                return false;

            // Get department first
            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department == null)
                return false;

            // Check if user is company admin or department manager for this company
            var role = await _unitOfWork.UserCompanyRoles.FirstOrDefaultAsync(ucr =>
                ucr.UserID == CurrentUserId &&
                ucr.CompanyID == department.CompanyID &&
                (ucr.Role == "CompanyAdmin" || ucr.Role == "DepartmentManager") &&
                ucr.IsActive
            );

            return role != null;
        }

        public async Task<CompanyConfiguration?> GetCompanyConfigAsync(int companyId)
        {
            var cacheKey = $"company_config_{companyId}";

            if (_cache.TryGetValue(cacheKey, out CompanyConfiguration? config))
                return config;

            config = await _unitOfWork.CompanyConfigurations.FirstOrDefaultAsync(c => c.CompanyID == companyId);

            if (config != null)
            {
                _cache.Set(cacheKey, config, TimeSpan.FromHours(1));
            }

            return config;
        }
    }
}
