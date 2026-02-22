using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class UpdateLayoutDTO
    {
        [Required(ErrorMessage = "Layout name is required")]
        [MaxLength(255)]
        public string LayoutName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Layout definition is required")]
        public string LayoutDefinition { get; set; } = string.Empty;
    }
}
