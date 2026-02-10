using System.Collections.Concurrent;
using System.Text.Json;

namespace DigitalSignage.Services
{
    /// <summary>
    /// JSON tabanlı dil paketi servisi.
    /// wwwroot/lang/ klasöründeki JSON dosyalarını okur, bellekte cache'ler ve
    /// nokta notasyonlu anahtarlar ile çevirilere erişim sağlar.
    /// </summary>
    public class LanguageService : ILanguageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<LanguageService> _logger;
        private readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache = new();
        private const string DEFAULT_LOCALE = "en";
        private const string LANG_FOLDER = "lang";

        public LanguageService(IWebHostEnvironment env, ILogger<LanguageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Belirtilen dil ve anahtar için çeviriyi döner.
        /// Anahtar bulunamazsa fallback olarak İngilizce denenir, o da yoksa anahtar döner.
        /// </summary>
        public string Get(string locale, string key)
        {
            if (string.IsNullOrEmpty(locale))
                locale = DEFAULT_LOCALE;

            var translations = LoadTranslations(locale);

            // Önce istenen dilde ara
            if (translations.TryGetValue(key, out var value))
                return value;

            // Fallback: varsayılan dilde ara
            if (locale != DEFAULT_LOCALE)
            {
                var fallback = LoadTranslations(DEFAULT_LOCALE);
                if (fallback.TryGetValue(key, out var fallbackValue))
                    return fallbackValue;
            }

            // Hiçbir yerde bulunamadıysa anahtarı döndür
            _logger.LogWarning("Translation key '{Key}' not found for locale '{Locale}'", key, locale);
            return key;
        }

        /// <summary>
        /// Belirtilen dilin tüm çevirilerini JSON string olarak döner.
        /// JavaScript tarafında kullanılmak üzere.
        /// </summary>
        public string GetAllAsJson(string locale)
        {
            if (string.IsNullOrEmpty(locale))
                locale = DEFAULT_LOCALE;

            var langPath = Path.Combine(_env.WebRootPath, LANG_FOLDER, $"{locale}.json");

            if (File.Exists(langPath))
            {
                return File.ReadAllText(langPath);
            }

            _logger.LogWarning("Language file not found: {Path}", langPath);
            return "{}";
        }

        /// <summary>
        /// Desteklenen dilleri wwwroot/lang/ klasöründeki dosyalardan alır.
        /// </summary>
        public IEnumerable<string> GetSupportedLanguages()
        {
            var langDir = Path.Combine(_env.WebRootPath, LANG_FOLDER);
            if (!Directory.Exists(langDir))
                return new[] { DEFAULT_LOCALE };

            return Directory.GetFiles(langDir, "*.json")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .OrderBy(l => l);
        }

        /// <summary>
        /// Dil dosyasını yükler ve düzleştirilmiş bir sözlüğe çevirir.
        /// Sonuç bellekte cache'lenir.
        /// </summary>
        private Dictionary<string, string> LoadTranslations(string locale)
        {
            return _cache.GetOrAdd(locale, key =>
            {
                var langPath = Path.Combine(_env.WebRootPath, LANG_FOLDER, $"{key}.json");
                var translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (!File.Exists(langPath))
                {
                    _logger.LogWarning("Language file not found: {Path}", langPath);
                    return translations;
                }

                try
                {
                    var json = File.ReadAllText(langPath);
                    using var doc = JsonDocument.Parse(json);
                    FlattenJson(doc.RootElement, "", translations);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading language file: {Path}", langPath);
                }

                return translations;
            });
        }

        /// <summary>
        /// JSON'u düzleştirir: {"nav": {"dashboard": "Panel"}} → {"nav.dashboard": "Panel"}
        /// </summary>
        private void FlattenJson(JsonElement element, string prefix, Dictionary<string, string> result)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        var newPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                        FlattenJson(property.Value, newPrefix, result);
                    }
                    break;
                case JsonValueKind.String:
                    result[prefix] = element.GetString() ?? prefix;
                    break;
                default:
                    result[prefix] = element.ToString();
                    break;
            }
        }
    }
}
