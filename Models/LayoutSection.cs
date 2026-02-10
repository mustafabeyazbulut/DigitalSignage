using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class LayoutSection
    {
        public int LayoutSectionID { get; set; }
        public int LayoutID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string SectionPosition { get; set; } = string.Empty; // A1, B2 etc.
        
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
        
        [MaxLength(50)]
        public string Width { get; set; } = "100%";
        
        [MaxLength(50)]
        public string Height { get; set; } = "100%";
        
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Layout Layout { get; set; } = null!;
        public ICollection<PageSection> PageSections { get; set; } = new List<PageSection>();
    }
}
