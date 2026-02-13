using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "User name is required")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }  // Optional for Office 365 users

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsOffice365User { get; set; }
        public string? AzureADObjectId { get; set; }
        public bool IsSystemAdmin { get; set; }
    }
}
