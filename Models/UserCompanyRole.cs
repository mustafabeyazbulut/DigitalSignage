using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class UserCompanyRole
    {
        public int UserCompanyRoleID { get; set; }
        public int UserID { get; set; }
        public int CompanyID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Viewer"; // SystemAdmin, CompanyAdmin, Manager, Editor, Viewer
        
        public bool IsActive { get; set; } = true;
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(255)]
        public string AssignedBy { get; set; } = "System";

        // Navigation Properties
        public User User { get; set; } = null!;
        public Company Company { get; set; } = null!;
    }
}
