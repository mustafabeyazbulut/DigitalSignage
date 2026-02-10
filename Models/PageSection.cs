using System;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class PageSection
    {
        public int PageSectionID { get; set; }
        public int PageID { get; set; }
        public int LayoutSectionID { get; set; }
        
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Page Page { get; set; } = null!;
        public LayoutSection LayoutSection { get; set; } = null!;
    }
}
