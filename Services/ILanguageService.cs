namespace DigitalSignage.Services
{
    /// <summary>
    /// JSON tabanlı dil paketi servisi arayüzü.
    /// wwwroot/lang/ altındaki JSON dosyalarını okur ve çevirileri sunar.
    /// </summary>
    public interface ILanguageService
    {
        /// <summary>
        /// Belirtilen dil ve anahtar için çeviriyi döner.
        /// Örnek: Get("en", "nav.dashboard") → "Dashboard"
        /// </summary>
        /// <param name="locale">Dil kodu (ör: "en", "tr")</param>
        /// <param name="key">Nokta notasyonlu anahtar (ör: "nav.dashboard")</param>
        /// <returns>Çevrilmiş metin veya anahtar</returns>
        string Get(string locale, string key);

        /// <summary>
        /// Aktif dilin tüm çevirilerini JSON string olarak döner (JS tarafı için).
        /// </summary>
        /// <param name="locale">Dil kodu</param>
        /// <returns>Tüm çeviriler JSON formatında</returns>
        string GetAllAsJson(string locale);

        /// <summary>
        /// Desteklenen dillerin listesini döner.
        /// </summary>
        /// <returns>Dil kodu listesi</returns>
        IEnumerable<string> GetSupportedLanguages();
    }
}
