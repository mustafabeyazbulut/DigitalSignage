using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class CreateDepartmentDTO
    {
        [Required(ErrorMessage = "Company ID is required")]
        public int CompanyID { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [MaxLength(255, ErrorMessage = "Department name cannot exceed 255 characters")]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public string CreatedBy { get; set; } = "System";
    }
}
