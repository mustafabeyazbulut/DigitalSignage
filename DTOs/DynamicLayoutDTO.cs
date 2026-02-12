using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    /// <summary>
    /// DTO for creating dynamic grid layouts with custom dimensions
    /// </summary>
    public class DynamicLayoutDTO
    {
        [Required(ErrorMessage = "Company ID is required")]
        public int CompanyID { get; set; }

        [Required(ErrorMessage = "Layout name is required")]
        [MaxLength(255)]
        public string LayoutName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Layout code is required")]
        [MaxLength(50)]
        public string LayoutCode { get; set; } = string.Empty;

        [Range(1, 12, ErrorMessage = "Grid columns must be between 1 and 12")]
        public int GridColumnsX { get; set; }

        [Range(1, 12, ErrorMessage = "Grid rows must be between 1 and 12")]
        public int GridRowsY { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string? CustomCSS { get; set; }
    }

    /// <summary>
    /// Request model for resizing existing grid layouts
    /// </summary>
    public class ResizeGridRequest
    {
        [Range(1, 12, ErrorMessage = "Grid columns must be between 1 and 12")]
        public int GridColumnsX { get; set; }

        [Range(1, 12, ErrorMessage = "Grid rows must be between 1 and 12")]
        public int GridRowsY { get; set; }
    }
}
