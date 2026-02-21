using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class CreatePageDTO
    {
        [Required(ErrorMessage = "Department is required")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Page name is required")]
        public string PageName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Page title is required")]
        public string PageTitle { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
