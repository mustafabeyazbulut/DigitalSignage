namespace DigitalSignage.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".mp4", ".webm", ".ogg", ".pdf" };

        public FileStorageService(IConfiguration configuration)
        {
            var relative = configuration["AppSettings:UploadsBasePath"] ?? "App_Data/uploads";
            _basePath = Path.Combine(Directory.GetCurrentDirectory(), relative);
        }

        public async Task<string> SaveFileAsync(IFormFile file, int companyId)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = Guid.NewGuid().ToString() + ext;
            var companyDir = Path.Combine(_basePath, companyId.ToString());

            Directory.CreateDirectory(companyDir);

            var filePath = Path.Combine(companyDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // DB'de saklanacak göreli yol: "{companyId}/{guid}.ext"
            return $"{companyId}/{fileName}";
        }

        public void DeleteFile(string? mediaPath)
        {
            if (string.IsNullOrEmpty(mediaPath))
                return;

            var fullPath = GetPhysicalPath(mediaPath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public string GetPhysicalPath(string mediaPath)
        {
            // mediaPath formatı: "{companyId}/{fileName}"
            return Path.Combine(_basePath, mediaPath.Replace('/', Path.DirectorySeparatorChar));
        }

        public bool IsValidFile(IFormFile file)
        {
            if (file == null || file.Length <= 0)
                return false;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(ext);
        }
    }
}
