# 11. Çok Dilli Destek (i18n / Localization)

## 📋 Genel Bakış

Digital Signage projesi, **JSON tabanlı dil paketi** mimarisi kullanır. Bu yaklaşım, ASP.NET'in geleneksel `.resx` resource dosyalarından çok daha esnek, yönetilebilir ve genişletilebilirdir.

### Neden JSON Tabanlı?
| Özellik | .resx (Geleneksel) | JSON (Tercih Edilen) |
|---------|-------------------|---------------------|
| Düzenleme | Visual Studio gerektirir | Herhangi bir editör |
| Yapı | Düz key-value | Hiyerarşik (nested) |
| Yeni dil ekleme | Derleme gerektirir | Dosya kopyala & çevir |
| Frontend desteği | Ek katman gerekir | Doğrudan JS'den okunabilir |
| Bakım | Zor | Kolay |

---

## 📁 Dosya Yapısı

```
DigitalSignage/
├── wwwroot/
│   └── lang/                          ← Dil paketleri
│       ├── en.json                    ← İngilizce (varsayılan / fallback)
│       ├── tr.json                    ← Türkçe
│       └── de.json                    ← Almanca
│
├── Services/
│   ├── ILanguageService.cs            ← Servis arayüzü
│   └── LanguageService.cs             ← JSON okuma, cache & çeviri servisi
│
├── Controllers/
│   ├── BaseController.cs              ← T() kısayolu, ViewBag.Lang aktarımı
│   └── LanguageController.cs          ← Dil değiştirme & JSON API
│
└── Views/
    └── Shared/
        └── _Layout.cshtml             ← Dil seçici dropdown
```

---

## 🔧 Mimari

### 1. LanguageService (Singleton)

`LanguageService` uygulamanın kalbindeki dil servisidir:

```csharp
public interface ILanguageService
{
    // Tek bir çeviri anahtarı getir
    string Get(string locale, string key);           // Ör: Get("tr", "nav.dashboard") → "Kontrol Paneli"
    
    // Tüm çevirileri JSON olarak getir (JS tarafı için)
    string GetAllAsJson(string locale);
    
    // Desteklenen dil listesi
    IEnumerable<string> GetSupportedLanguages();
}
```

**Özellikler:**
- `ConcurrentDictionary` ile thread-safe bellekte cache
- Nokta notasyonlu düzleştirme: `{"nav": {"dashboard": "Panel"}}` → `"nav.dashboard": "Panel"`
- Fallback: İstenen dilde bulunamazsa → İngilizce → Anahtar döner
- Singleton lifecycle: Uygulama ömrü boyunca 1 kez yüklenir

### 2. BaseController (Tüm Controller'ların Temeli)

```csharp
public class BaseController : Controller
{
    protected ILanguageService? _languageService;
    protected string CurrentLocale => HttpContext?.Request.Cookies["locale"] ?? "en";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        _languageService = HttpContext.RequestServices.GetService<ILanguageService>();
        ViewBag.CurrentLocale = CurrentLocale;
        ViewBag.Lang = _languageService;
        ViewBag.SupportedLanguages = _languageService?.GetSupportedLanguages();
    }

    // Controller'dan çeviri kısayolu
    protected string T(string key) => _languageService?.Get(CurrentLocale, key) ?? key;
}
```

### 3. LanguageController (Dil Değiştirme)

```
GET /Language/Change?locale=tr&returnUrl=/Home/Index
```

- Cookie'ye `locale=tr` yazar (1 yıl geçerli)
- `returnUrl`'e yönlendirir
- `[AllowAnonymous]` → Login sayfasında da çalışır

```
GET /Language/Json?locale=tr
```

- Tüm çevirileri JSON olarak döner (AJAX/JS kullanımı için)

---

## 📝 Dil Paketi Yapısı (JSON)

Her dil paketi aynı anahtar yapısını kullanır:

```json
{
  "common": {
    "appName": "Digital Signage",
    "save": "Save",
    "cancel": "Cancel",
    "delete": "Delete",
    "active": "Active",
    "inactive": "Inactive"
  },
  "auth": {
    "login": "Sign In",
    "logout": "Logout",
    "email": "Corporate Email / Username",
    "password": "Password"
  },
  "nav": {
    "dashboard": "Dashboard",
    "companies": "Companies",
    "departments": "Departments"
  },
  "company": {
    "title": "Companies",
    "subtitle": "Manage client organizations.",
    "newCompany": "New Company"
  }
}
```

### Bölüm (Section) Yapısı

| Bölüm | Açıklama | Kullanıldığı Yer |
|-------|----------|-----------------|
| `common` | Genel terimler (kaydet, iptal, sil vb.) | Tüm sayfalar |
| `auth` | Kimlik doğrulama metinleri | Login sayfası |
| `nav` | Sidebar ve navigasyon menüsü | _Layout.cshtml |
| `dashboard` | Ana sayfa / kontrol paneli | Home/Index |
| `company` | Şirket yönetimi | Company/* |
| `department` | Departman yönetimi | Department/* |
| `page` | Sayfa yönetimi | Page/* |
| `layout` | Düzen/grid yönetimi | Layout/* |
| `content` | Medya kütüphanesi | Content/* |
| `schedule` | Zamanlama yönetimi | Schedule/* |
| `language` | Dil adları | Dil seçici |
| `profile` | Kullanıcı profili | Account/Profile |
| `settings` | Kullanıcı ayarları | Account/Settings |
| `role` | Rol yönetimi | User/ManageRoles |

---

## 🖥️ View'larda Kullanım

### Razor View'larda T() Fonksiyonu

Her View'un başında şu blok tanımlanır:

```cshtml
@{
    var locale = ViewBag.CurrentLocale as string ?? "en";
    var lang = ViewBag.Lang as DigitalSignage.Services.ILanguageService;
    string T(string key) => lang?.Get(locale, key) ?? key;
    ViewData["Title"] = T("company.title");
}

<!-- Kullanım -->
<h2>@T("company.title")</h2>
<p>@T("company.subtitle")</p>
<button>@T("common.save")</button>
```

> ⚠️ **ÖNEMLİ**: `T()` fonksiyonu, `locale` ve `lang` değişkenlerinden **SONRA** tanımlanmalıdır. Aksi halde "Use of unassigned local variable" hatası alırsınız.

### Login Sayfasında (Layout = null)

Login sayfası `BaseController`'dan geçmediği için `@inject` kullanılır:

```cshtml
@inject DigitalSignage.Services.ILanguageService LangService

@{
    Layout = null;
    var locale = Context.Request.Cookies["locale"] ?? "en";
    string T(string key) => LangService.Get(locale, key);
}
```

### Controller'larda T() Kullanımı

```csharp
public class CompanyController : BaseController
{
    public async Task<IActionResult> Create(Company company)
    {
        // ... iş mantığı ...
        AddSuccessMessage(T("company.createdSuccess"));
        return RedirectToAction("Index");
    }
}
```

---

## 🌐 Dil Seçici (Language Switcher)

### Layout Dil Seçici (Header)

`_Layout.cshtml` içinde Bootstrap dropdown olarak yer alır:

```cshtml
<li class="nav-item dropdown me-3">
    <a class="nav-link dropdown-toggle" href="#" data-bs-toggle="dropdown">
        <i class="fas fa-globe me-1 text-primary"></i>
        <span>English</span>
    </a>
    <div class="dropdown-menu dropdown-menu-end">
        @foreach (var l in supportedLangs)
        {
            <a asp-controller="Language" asp-action="Change" 
               asp-route-locale="@l" 
               asp-route-returnUrl="@Context.Request.Path"
               class="dropdown-item @(isActive ? "active" : "")">
                🇺🇸 English ✓
            </a>
        }
    </div>
</li>
```

### Login Sayfası Dil Seçici

CSS hover-based dropdown kullanır. **Önemli:** Hover-gap sorununu önlemek için `::before` pseudo-element ile görünmez köprü eklenir:

```css
.dropdown-menu::before {
    content: '';
    position: absolute;
    top: -10px;
    left: 0;
    right: 0;
    height: 10px;
}
```

---

## ➕ Yeni Dil Ekleme Rehberi

### Adım 1: JSON Dosyası Oluştur
```bash
# en.json'u kopyala
cp wwwroot/lang/en.json wwwroot/lang/fr.json
```

### Adım 2: Tüm Değerleri Çevir
```json
{
  "common": {
    "save": "Enregistrer",
    "cancel": "Annuler"
  }
}
```

### Adım 3: Bayrak & İsim Ekle

`_Layout.cshtml` ve `Login.cshtml` dosyalarındaki sözlüklere ekle:

```csharp
var langFlags = new Dictionary<string, string> {
    { "en", "🇺🇸" },
    { "tr", "🇹🇷" },
    { "de", "🇩🇪" },
    { "fr", "🇫🇷" }  // ← YENİ
};

var langNames = new Dictionary<string, string> {
    { "en", "English" },
    { "tr", "Türkçe" },
    { "de", "Deutsch" },
    { "fr", "Français" }  // ← YENİ
};
```

### Adım 4: Test Et
- JSON dosyası otomatik olarak `GetSupportedLanguages()` tarafından algılanır
- Dil seçiciye otomatik eklenir
- Uygulama yeniden başlatmaya gerek yoktur (dosya cache'den okunuyorsa restart gerekebilir)

---

## 🔄 Dil Değiştirme Akışı

```
┌────────────────┐     ┌─────────────────────┐     ┌──────────────────┐
│ Kullanıcı       │────▶│ GET /Language/Change │────▶│ Cookie set:       │
│ Dil seçer       │     │ ?locale=tr           │     │ locale=tr (1 yıl)│
└────────────────┘     │ &returnUrl=/Home     │     └──────────────────┘
                       └─────────────────────┘              │
                                                            ▼
                       ┌─────────────────────┐     ┌──────────────────┐
                       │ View render edilir   │◀────│ Redirect:         │
                       │ T("nav.dashboard")   │     │ /Home/Index      │
                       │ → "Kontrol Paneli"   │     └──────────────────┘
                       └─────────────────────┘
```

---

## ⚠️ Dikkat Edilecekler

1. **Anahtar tutarlılığı**: Tüm dil dosyalarında aynı anahtarlar bulunmalıdır. Eksik anahtar varsa İngilizce fallback devreye girer.
2. **View sıralama**: `T()` fonksiyonu, `locale` ve `lang` değişkenlerinden sonra tanımlanmalıdır.
3. **Cache**: `LanguageService` Singleton olarak çalışır. JSON dosyası değiştiğinde uygulamayı yeniden başlatın.
4. **Login sayfası**: `BaseController`'dan geçmediği için `@inject` ile DI kullanılır.
5. **Özel karakterler**: JSON dosyalarında Türkçe karakterler (ş, ğ, ü, ı, ö, ç) UTF-8 olarak saklanır.

---

## 🧪 Test Senaryoları

| Senaryo | Beklenen Davranış |
|---------|-------------------|
| Dil seçici tıklama | Cookie set edilir, sayfa yeniden yüklenir |
| Desteklenmeyen dil | İngilizce'ye fallback yapılır |
| Eksik çeviri anahtarı | İngilizce'den fallback, yoksa anahtar döner |
| Login sayfasında dil değiştirme | Unauthenticated kullanıcı da değiştirebilir |
| Tarayıcı kapandıktan sonra | Cookie 1 yıl geçerli, tercih hatırlanır |
| Yeni dil dosyası ekleme | Uygulama restart sonrası otomatik algılanır |

---

## 🔄 Son Güncellemeler

### v2.2.2 (13 Şubat 2026)
- ✅ **Email-Based Authentication (Multi-Tenant Güvenlik)**
  - Username yerine email ile kimlik doğrulama
  - IUserService.AuthenticateAsync metodu email parametresi kullanıyor
  - AccountController.Login metodu email ile authentication yapıyor
  - Aynı username'in farklı şirketlerde olması problemi çözüldü
- ✅ **Login Form Validasyonu**
  - Input type="email" ile HTML5 native validation
  - Email formatı zorunlu (@domain.com gerekli)
  - auth.email ve auth.emailPlaceholder anahtarları kullanılıyor
- ✅ **CompanySelector Localization**
  - 3 yeni çeviri anahtarı eklendi (company.*)
  - selectCompany, switchCompany, noCompaniesAvailable
  - Dropdown header ve placeholder'lar tamamen localized
- ✅ **Auth Translations Güncelleme**
  - invalidCredentials: "username" → "email"
  - requiredFields: "username" → "email"
  - Tüm dillerde (EN, TR, DE) güncellemeler yapıldı

### v2.2.1 (12 Şubat 2026)
- ✅ **Profil Sayfası Yerelleştirme**: 6 yeni çeviri anahtarı (profile.*)
  - systemAdmin, personalInfo, lastLogin, office365Info, changePasswordInfo, contactAdmin
  - Extensions bağımlılığı kaldırıldı, doğrudan User modeli kullanılıyor
- ✅ **Ayarlar Sayfası Yerelleştirme**: 15 yeni çeviri anahtarı (settings.*)
  - Email bildirim ayarları eklendi (EmailNotificationsEnabled)
  - Browser notifications kaldırıldı
  - Fonksiyonel toggle ve kaydetme özelliği
- ✅ **Login Sayfası Düzeltme**: Email etiketi username olarak değiştirildi
  - auth.username, auth.usernamePlaceholder anahtarları eklendi

### v2.2.0 (12 Şubat 2026)
- ✅ **Rol Yönetimi Yerelleştirme**: 51 yeni çeviri anahtarı (role.*)
  - Şirket ve departman seviyesi rol atama arayüzleri
  - AJAX tabanlı dinamik departman yükleme

---

**Son Güncelleme**: 13 Şubat 2026
