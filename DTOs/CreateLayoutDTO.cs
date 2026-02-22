using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class CreateLayoutDTO
    {
        [Required(ErrorMessage = "Company ID is required")]
        public int CompanyID { get; set; }

        [Required(ErrorMessage = "Layout name is required")]
        [MaxLength(255, ErrorMessage = "Layout name cannot exceed 255 characters")]
        public string LayoutName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Layout definition is required")]
        public string LayoutDefinition { get; set; } = "{\"rows\":[{\"height\":100,\"columns\":[{\"width\":100}]}]}";

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }
}
