using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    public class CreateContentDTO
    {
        [Required(ErrorMessage = "Department is required")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Content type is required")]
        public string ContentType { get; set; } = string.Empty;  // Text, Image, Video, HTML, PDF, URL

        public string ContentTitle { get; set; } = string.Empty;

        public string? ContentData { get; set; }

        public string? MediaPath { get; set; }
    }
}
