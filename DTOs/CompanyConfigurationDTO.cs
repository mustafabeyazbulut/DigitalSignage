namespace DigitalSignage.DTOs
{
    public class CompanyConfigurationDTO
    {
        public int DefaultGridColumnsX { get; set; } = 2;
        public int DefaultGridRowsY { get; set; } = 2;
        public string DefaultSectionPadding { get; set; } = "10px";
        public int ScreenRefreshInterval { get; set; } = 5;
        public bool EnableAutoRotation { get; set; } = true;
        public string? CustomCSS { get; set; }
        public bool EnableAnalytics { get; set; } = true;
        public bool EnableAdvancedScheduling { get; set; } = true;
        public bool EnableMediaUpload { get; set; } = true;
        public int MaxMediaSizeGB { get; set; } = 10;
    }
}
