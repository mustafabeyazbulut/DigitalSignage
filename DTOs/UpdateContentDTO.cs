using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class UpdateContentDTO
    {
        [MaxLength(255)]
        public string? ContentTitle { get; set; }

        [Required(ErrorMessage = "Content data is required")]
        public string ContentData { get; set; } = string.Empty;

        public string? MediaPath { get; set; }

        [Range(1, 3600, ErrorMessage = "Duration must be between 1 and 3600 seconds")]
        public int? DurationSeconds { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
