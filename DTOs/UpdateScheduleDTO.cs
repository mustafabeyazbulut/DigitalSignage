using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class UpdateScheduleDTO
    {
        [Required(ErrorMessage = "Schedule name is required")]
        [MaxLength(255)]
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
        public string? RecurrencePattern { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
