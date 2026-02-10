using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalSignage.Models
{
    public class Content
    {
        public int ContentID { get; set; }
        public int DepartmentID { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string ContentType { get; set; } = "Text"; // Text, Image, Video, HTML
        
        [MaxLength(255)]
        public string? ContentTitle { get; set; }
        
        public string? ContentData { get; set; } // HTML or Text content
        
        [MaxLength(500)]
        public string? MediaPath { get; set; }
        
        [MaxLength(500)]
        public string? ThumbnailPath { get; set; }
        
        public int? DurationSeconds { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [MaxLength(255)]
        public string CreatedBy { get; set; } = "System";
        
        public DateTime? ModifiedDate { get; set; }

        // Navigation Properties
        public Department Department { get; set; } = null!;
        public ICollection<PageContent> PageContents { get; set; } = new List<PageContent>();
    }
}
