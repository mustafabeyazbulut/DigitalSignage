using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }
        public int CompanyID { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string DepartmentName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string DepartmentCode { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(255)]
        public string CreatedBy { get; set; } = "System";

        // Navigation Properties
        public Company Company { get; set; } = null!;
        public ICollection<Page> Pages { get; set; } = new List<Page>();
        public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public ICollection<Content> Contents { get; set; } = new List<Content>();
        public ICollection<UserDepartmentRole> UserDepartmentRoles { get; set; } = new List<UserDepartmentRole>();
    }
}
