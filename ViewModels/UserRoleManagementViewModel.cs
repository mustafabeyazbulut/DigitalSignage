using DigitalSignage.Models;
using System.Collections.Generic;

namespace DigitalSignage.ViewModels
{
    /// <summary>
    /// Kullanıcı rol yönetimi sayfası için ViewModel
    /// </summary>
    public class UserRoleManagementViewModel
    {
        public User User { get; set; } = null!;
        public List<UserCompanyRoleViewModel> CompanyRoles { get; set; } = new();
        public List<UserDepartmentRoleViewModel> DepartmentRoles { get; set; } = new();
        public List<Company> AvailableCompanies { get; set; } = new();
        public List<Department> AvailableDepartments { get; set; } = new();
    }
}
