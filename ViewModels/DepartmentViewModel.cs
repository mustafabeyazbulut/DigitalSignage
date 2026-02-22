namespace DigitalSignage.ViewModels
{
    public class DepartmentViewModel
    {
        public int DepartmentID { get; set; }
        public int CompanyID { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int PageCount { get; set; }
        public int ContentCount { get; set; }
        public int ScheduleCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
