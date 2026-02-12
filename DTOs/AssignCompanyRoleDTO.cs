using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    /// <summary>
    /// Kullanıcıya şirket rolü atamak için DTO
    /// </summary>
    public class AssignCompanyRoleDTO
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public int CompanyID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Viewer"; // CompanyAdmin, Viewer
    }
}
