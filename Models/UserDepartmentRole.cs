using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    /// <summary>
    /// Kullanıcı-Departman rol ilişkisi için entity
    /// Bir kullanıcının belirli departmanlardaki rolünü tanımlar
    /// </summary>
    public class UserDepartmentRole
    {
        public int UserDepartmentRoleID { get; set; }
        public int UserID { get; set; }
        public int DepartmentID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Viewer"; // DepartmentManager, Editor, Viewer

        public bool IsActive { get; set; } = true;
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string AssignedBy { get; set; } = "System";

        // Navigation Properties
        public User User { get; set; } = null!;
        public Department Department { get; set; } = null!;
    }
}
