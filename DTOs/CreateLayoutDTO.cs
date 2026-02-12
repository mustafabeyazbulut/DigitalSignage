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

        [Required(ErrorMessage = "Layout code is required")]
        [MaxLength(50, ErrorMessage = "Layout code cannot exceed 50 characters")]
        public string LayoutCode { get; set; } = string.Empty;

        [Range(1, 12, ErrorMessage = "Grid columns must be between 1 and 12")]
        public int GridColumnsX { get; set; } = 2;

        [Range(1, 12, ErrorMessage = "Grid rows must be between 1 and 12")]
        public int GridRowsY { get; set; } = 2;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }
}
