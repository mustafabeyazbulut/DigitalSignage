namespace DigitalSignage.DTOs
{
    public class UserPhotoDTO
    {
        public byte[] PhotoBytes { get; set; } = Array.Empty<byte>();
        public string? ContentType { get; set; }
    }
}
