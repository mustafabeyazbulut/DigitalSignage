namespace DigitalSignage.ViewModels
{
    public class LayoutViewModel
    {
        public int LayoutID { get; set; }
        public int CompanyID { get; set; }
        public string LayoutName { get; set; } = string.Empty;
        public string LayoutDefinition { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalSections { get; set; }
        public int RowCount { get; set; }
    }
}
