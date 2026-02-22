using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class UpdateContentDTO
    {
        [MaxLength(255)]
        public string? ContentTitle { get; set; }

        public string? ContentData { get; set; }

        public string? MediaPath { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
