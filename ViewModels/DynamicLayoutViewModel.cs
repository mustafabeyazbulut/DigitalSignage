namespace DigitalSignage.ViewModels
{
    public class DynamicLayoutViewModel
    {
        public int LayoutID { get; set; }
        public int CompanyID { get; set; }
        public string LayoutName { get; set; } = string.Empty;

        // Dynamic Grid Configuration
        public int GridColumnsX { get; set; }  // 1-12
        public int GridRowsY { get; set; }     // 1-12

        // Sections with coordinates
        public List<GridSectionDTO> Sections { get; set; } = new();

        public class GridSectionDTO
        {
            public int SectionID { get; set; }
            public string Position { get; set; } = string.Empty;
            public int Column { get; set; }
            public int Row { get; set; }
            public string Width { get; set; } = "100%";
            public string Height { get; set; } = "100%";
        }
    }
}
