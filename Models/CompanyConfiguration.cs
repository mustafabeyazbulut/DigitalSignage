using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class CompanyConfiguration
    {
        public int ConfigurationID { get; set; }
        public int CompanyID { get; set; }

        // Email Settings
        [MaxLength(255)]
        public string? EmailSmtpServer { get; set; }
        public int EmailSmtpPort { get; set; } = 587;
        
        [MaxLength(255)]
        public string? EmailFrom { get; set; }
        
        [MaxLength(255)]
        public string? EmailUsername { get; set; }
        
        [MaxLength(500)]
        public string? EmailPassword { get; set; }
        public bool EmailNotificationsEnabled { get; set; }

        // Notification Settings
        public bool NotifyOnScheduleChange { get; set; }
        public bool NotifyOnContentChange { get; set; }
        public bool NotifyOnError { get; set; }
        
        [MaxLength(255)]
        public string? NotificationEmail { get; set; }

        // Layout Defaults
        public int DefaultGridColumnsX { get; set; } = 2;
        public int DefaultGridRowsY { get; set; } = 2;
        
        [MaxLength(50)]
        public string DefaultSectionPadding { get; set; } = "10px";
        
        [MaxLength(50)]
        public string DefaultSectionBorder { get; set; } = "1px solid #ddd";

        // Schedule Rules
        public int MaxSchedulesPerPage { get; set; } = 10;
        public int DefaultScheduleDuration { get; set; } = 30; // seconds
        public bool AllowRecurringSchedules { get; set; } = true;

        // Display Settings
        public int ScreenRefreshInterval { get; set; } = 5; // seconds
        public bool EnableAutoRotation { get; set; } = true;
        public string? CustomCSS { get; set; }

        // Feature Flags
        public bool EnableAnalytics { get; set; } = true;
        public bool EnableAdvancedScheduling { get; set; } = true;
        public bool EnableMediaUpload { get; set; } = true;
        public int MaxMediaSizeGB { get; set; } = 10;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public Company Company { get; set; } = null!;
    }
}
