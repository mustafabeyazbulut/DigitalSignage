namespace DigitalSignage.ViewModels
{
    public class LayoutViewModel
    {
        public int LayoutID { get; set; }
        public int CompanyID { get; set; }
        public string LayoutName { get; set; } = string.Empty;
        public string LayoutCode { get; set; } = string.Empty;
        public int GridColumnsX { get; set; }
        public int GridRowsY { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalSections => GridColumnsX * GridRowsY;
    }
}
