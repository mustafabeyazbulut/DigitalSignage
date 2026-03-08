using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class Schedule
    {
        public int ScheduleID { get; set; }
        public int CompanyID { get; set; }

        [Required]
        [MaxLength(255)]
        public string ScheduleName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Company Company { get; set; } = null!;
        public ICollection<SchedulePage> SchedulePages { get; set; } = new List<SchedulePage>();
    }
}
