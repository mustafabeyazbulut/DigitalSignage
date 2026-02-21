using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class UpdatePageDTO
    {
        [Required(ErrorMessage = "Page name is required")]
        [MaxLength(255)]
        public string PageName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Page title is required")]
        [MaxLength(255)]
        public string PageTitle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
