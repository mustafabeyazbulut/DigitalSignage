using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class User
    {
        public int UserID { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string? PasswordHash { get; set; }  // Office 365 kullanıcıları için NULL olabilir
        
        [MaxLength(255)]
        public string? FirstName { get; set; }
        
        [MaxLength(255)]
        public string? LastName { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsSystemAdmin { get; set; }
        public bool IsOffice365User { get; set; }
        public bool EmailNotificationsEnabled { get; set; } = true;
        
        [MaxLength(255)]
        public string? AzureADObjectId { get; set; }
        
        [MaxLength(500)]
        public string? PhotoUrl { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public ICollection<UserCompanyRole> UserCompanyRoles { get; set; } = new List<UserCompanyRole>();
        public ICollection<UserDepartmentRole> UserDepartmentRoles { get; set; } = new List<UserDepartmentRole>();
    }
}
