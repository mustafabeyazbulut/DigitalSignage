namespace DigitalSignage.DTOs
{
    public class UserEmailDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public DateTime ReceivedDateTime { get; set; }
    }
}
