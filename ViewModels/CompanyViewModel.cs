namespace DigitalSignage.ViewModels
{
    public class CompanyViewModel
    {
        public int CompanyID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int DepartmentCount { get; set; }
        public int LayoutCount { get; set; }
public DateTime CreatedDate { get; set; }
    }
}
