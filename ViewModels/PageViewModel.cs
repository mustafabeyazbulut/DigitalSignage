namespace DigitalSignage.ViewModels
{
    public class PageViewModel
    {
        public int PageID { get; set; }
        public int DepartmentID { get; set; }
        public string PageName { get; set; } = string.Empty;
        public string PageTitle { get; set; } = string.Empty;
        public int LayoutID { get; set; }
        public string LayoutName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ContentCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
