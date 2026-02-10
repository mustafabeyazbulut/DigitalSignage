using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class PageContent
    {
        public int PageContentID { get; set; }
        public int PageID { get; set; }
        public int ContentID { get; set; }
        
        public int DisplayOrder { get; set; }
        
        [MaxLength(50)]
        public string? DisplaySection { get; set; } // Hangi section'da
        
        public bool IsActive { get; set; } = true;
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Page Page { get; set; } = null!;
        public Content Content { get; set; } = null!;
    }
}
