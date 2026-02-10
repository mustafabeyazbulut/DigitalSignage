using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class SchedulePage
    {
        public int SchedulePageID { get; set; }
        public int ScheduleID { get; set; }
        public int PageID { get; set; }
        
        public int DisplayDuration { get; set; } = 30; // Seconds
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Schedule Schedule { get; set; } = null!;
        public Page Page { get; set; } = null!;
    }
}
