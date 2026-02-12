using System;

namespace DigitalSignage.ViewModels
{
    /// <summary>
    /// Department role için display model
    /// </summary>
    public class UserDepartmentRoleViewModel
    {
        public int UserDepartmentRoleID { get; set; }
        public int UserID { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}
