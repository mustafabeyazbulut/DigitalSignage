using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class UpdateScheduleDTO
    {
        [Required(ErrorMessage = "Schedule name is required")]
        [MaxLength(255)]
        public string ScheduleName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
