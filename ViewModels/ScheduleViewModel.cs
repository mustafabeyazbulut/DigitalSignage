namespace DigitalSignage.ViewModels
{
    public class ScheduleViewModel
    {
        public int ScheduleID { get; set; }
        public int CompanyID { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int PageCount { get; set; }
    }
}
