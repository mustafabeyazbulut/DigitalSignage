using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class UpdateDepartmentDTO
    {
        [Required(ErrorMessage = "Department name is required")]
        [MaxLength(255)]
        public string DepartmentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department code is required")]
        [MaxLength(50)]
        public string DepartmentCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
