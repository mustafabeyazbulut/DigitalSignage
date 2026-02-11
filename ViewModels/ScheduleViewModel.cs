namespace DigitalSignage.ViewModels
{
    public class ScheduleViewModel
    {
        public int ScheduleID { get; set; }
        public int DepartmentID { get; set; }
        public string ScheduleName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public bool IsActive { get; set; }
        public int PageCount { get; set; }
    }
}
