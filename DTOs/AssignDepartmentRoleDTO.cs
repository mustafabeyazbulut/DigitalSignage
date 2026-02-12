using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    /// <summary>
    /// Kullanıcıya departman rolü atamak için DTO
    /// </summary>
    public class AssignDepartmentRoleDTO
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public int DepartmentID { get; set; }

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Viewer"; // DepartmentManager, Editor, Viewer
    }
}
