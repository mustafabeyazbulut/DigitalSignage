using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class CreateScheduleDTO
    {
        [Required(ErrorMessage = "Department ID is required")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Schedule name is required")]
        [MaxLength(255, ErrorMessage = "Schedule name cannot exceed 255 characters")]
        public string ScheduleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public TimeSpan EndTime { get; set; }

        public bool IsRecurring { get; set; }

        [MaxLength(50)]
        public string? RecurrencePattern { get; set; }  // Daily, Weekly, Monthly

        public List<int> PageIds { get; set; } = new();

        public int DefaultDisplayDuration { get; set; } = 30;  // seconds
    }
}
