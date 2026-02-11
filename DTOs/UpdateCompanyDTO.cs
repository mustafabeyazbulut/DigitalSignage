namespace DigitalSignage.DTOs
{
    public class UpdateCompanyDTO
    {
        public string CompanyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
