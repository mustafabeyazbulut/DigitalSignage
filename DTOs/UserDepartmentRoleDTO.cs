using System;

namespace DigitalSignage.DTOs
{
    /// <summary>
    /// UserDepartmentRole entity için DTO
    /// </summary>
    public class UserDepartmentRoleDTO
    {
        public int UserDepartmentRoleID { get; set; }
        public int UserID { get; set; }
        public int DepartmentID { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime AssignedDate { get; set; }
        public string AssignedBy { get; set; } = string.Empty;

        // Display properties
        public string UserName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
    }
}
