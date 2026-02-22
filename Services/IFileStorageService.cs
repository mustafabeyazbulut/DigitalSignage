namespace DigitalSignage.Services
{
    /// <summary>
    /// Dosya yükleme, silme ve erişim işlemlerini yöneten servis.
    /// Dosyalar wwwroot dışında App_Data/uploads/{companyId}/ altında saklanır.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Dosyayı diske kaydeder.
        /// </summary>
        /// <returns>Göreli medya yolu, örn: "3/abc123.jpg"</returns>
        Task<string> SaveFileAsync(IFormFile file, int companyId);

        /// <summary>
        /// Fiziksel dosyayı siler.
        /// </summary>
        void DeleteFile(string? mediaPath);

        /// <summary>
        /// Göreli medya yolundan tam disk yolunu döner.
        /// </summary>
        string GetPhysicalPath(string mediaPath);

        /// <summary>
        /// Dosya uzantısının izin verilen listede olup olmadığını kontrol eder.
        /// </summary>
        bool IsValidFile(IFormFile file);
    }
}
