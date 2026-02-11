namespace DigitalSignage.DTOs
{
    public class UserProfileDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Mail { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string? MobilePhone { get; set; }
        public string? OfficeLocation { get; set; }
    }
}
