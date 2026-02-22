using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.DTOs
{
    /// <summary>
    /// Özel satır/sütun tanımlı dinamik grid düzenleri oluşturmak için DTO
    /// </summary>
    public class DynamicLayoutDTO
    {
        [Required(ErrorMessage = "Company ID is required")]
        public int CompanyID { get; set; }

        [Required(ErrorMessage = "Layout name is required")]
        [MaxLength(255)]
        public string LayoutName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Layout definition is required")]
        public string LayoutDefinition { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
