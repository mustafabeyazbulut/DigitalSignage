using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    /// <summary>
    /// Kullanıcı güncelleme için Data Transfer Object
    /// </summary>
    public class UpdateUserDTO
    {
        [Required(ErrorMessage = "User name is required")]
        [MaxLength(255)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? FirstName { get; set; }

        [MaxLength(255)]
        public string? LastName { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsSystemAdmin { get; set; }

        public bool EmailNotificationsEnabled { get; set; } = true;
    }
}
