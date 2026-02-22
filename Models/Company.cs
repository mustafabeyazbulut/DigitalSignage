using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class Company
    {
        public int CompanyID { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string CompanyName { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string? EmailDomain { get; set; }
        
        [MaxLength(500)]
        public string? LogoPath { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(255)]
        public string CreatedBy { get; set; } = "System";
        
        public DateTime? ModifiedDate { get; set; }
        
        [MaxLength(255)]
        public string? ModifiedBy { get; set; }

        // Navigation Properties
        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<Layout> Layouts { get; set; } = new List<Layout>();
        public ICollection<UserCompanyRole> UserCompanyRoles { get; set; } = new List<UserCompanyRole>();
        public CompanyConfiguration? Configuration { get; set; }
    }
}
