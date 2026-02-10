using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class Layout
    {
        public int LayoutID { get; set; }
        public int CompanyID { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string LayoutName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string LayoutCode { get; set; } = string.Empty;
        
        public int GridColumnsX { get; set; } = 1;
        public int GridRowsY { get; set; } = 1;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Company Company { get; set; } = null!;
        public ICollection<LayoutSection> LayoutSections { get; set; } = new List<LayoutSection>();
        public ICollection<Page> Pages { get; set; } = new List<Page>();
    }
}
