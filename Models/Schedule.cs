using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class Schedule
    {
        public int ScheduleID { get; set; }
        public int DepartmentID { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string ScheduleName { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        
        public bool IsRecurring { get; set; }
        
        [MaxLength(50)]
        public string? RecurrencePattern { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Department Department { get; set; } = null!;
        public ICollection<SchedulePage> SchedulePages { get; set; } = new List<SchedulePage>();
    }
}
