using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class CreateScheduleDTO
    {
        [Required(ErrorMessage = "Company ID is required")]
        public int CompanyID { get; set; }

        [Required(ErrorMessage = "Schedule name is required")]
        [MaxLength(255, ErrorMessage = "Schedule name cannot exceed 255 characters")]
        public string ScheduleName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public List<int> PageIds { get; set; } = new();
        public int DefaultDisplayDuration { get; set; } = 30;
    }
}
