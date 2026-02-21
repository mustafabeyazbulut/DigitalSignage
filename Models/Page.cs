using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class Page
    {
        public int PageID { get; set; }
        public int DepartmentID { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string PageName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string PageTitle { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string PageCode { get; set; } = string.Empty;
        
        public int? LayoutID { get; set; }
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public Department Department { get; set; } = null!;
        public Layout? Layout { get; set; }
        public ICollection<PageContent> PageContents { get; set; } = new List<PageContent>();
        public ICollection<PageSection> PageSections { get; set; } = new List<PageSection>();
        public ICollection<SchedulePage> SchedulePages { get; set; } = new List<SchedulePage>();
    }
}
