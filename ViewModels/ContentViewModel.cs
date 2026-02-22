namespace DigitalSignage.ViewModels
{
    public class ContentViewModel
    {
        public int ContentID { get; set; }
        public int DepartmentID { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string ContentTitle { get; set; } = string.Empty;
        public string ContentData { get; set; } = string.Empty;
        public string? MediaPath { get; set; }
        public string? ThumbnailPath { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
