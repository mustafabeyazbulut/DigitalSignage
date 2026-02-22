using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class CreateCompanyDTO
    {
        [Required(ErrorMessage = "Company name is required")]
        [MaxLength(255, ErrorMessage = "Company name cannot exceed 255 characters")]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email domain format")]
        public string? EmailDomain { get; set; }

        public string CreatedBy { get; set; } = "System";
    }
}
