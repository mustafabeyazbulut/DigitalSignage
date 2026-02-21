# CLAUDE.md

Bu dosya, Claude Code'un (claude.ai/code) bu repoda çalışırken başvuracağı rehberdir.

## Proje Özeti

Çok kiracılı (multi-tenant) dijital tabela yönetim sistemi. ASP.NET Core 9 MVC, SQL Server, EF Core 9 (code-first), AutoMapper, FluentValidation, BCrypt, Serilog ve opsiyonel Azure AD/Office365 SSO kullanır.

## Komutlar

```bash
dotnet restore
dotnet build
dotnet run                             # Dev: DB'yi otomatik migrate eder, SystemAdmin + örnek veri ekler
dotnet run --environment Production
dotnet ef database update              # Bekleyen migration'ları uygula
dotnet ef migrations add <İsim>        # Yeni migration oluştur
```

Varsayılan geliştirme URL'i: `http://localhost:5259`. İlk çalıştırmada seeded `admin@digitalsignage.com` şifresi konsola yazdırılır.

Bağlantı dizesi `appsettings.Development.json` dosyasına yazılır (gitignored). `appsettings.json` içindeki `DefaultConnection` kasıtlı olarak boş bırakılmıştır. Azure AD SSO yalnızca `AzureAd:ClientId` dolu olduğunda etkinleşir.

## Mimari

### İstek Pipeline'ı

```
HTTP → HTTPS Yönlendirme → Güvenlik Başlıkları → Session → Routing →
Kimlik Doğrulama (Cookie/OIDC) → Yetkilendirme → Antiforgery →
TenantResolverMiddleware → Controller'lar
```

`TenantResolverMiddleware` kimlik doğrulamadan sonra çalışır ve `CompanyId`'yi route parametresinden veya session'dan alarak `HttpContext.Items["CompanyId"]`'ye yazar.

### Katmanlar

```
Controller (ince, BaseController'dan türetilmiş)
  └─ Service (iş mantığı: IUserService vb.)
       └─ IUnitOfWork (tüm repository'leri toplar, transaction yönetir)
            └─ Repository<T> (generic: GetByIdAsync, FindAsync, GetPagedAsync, Query())
                 └─ AppDbContext (EF Core)
```

Tüm servisler ve UoW `AddScoped`'dır. `ILanguageService` `AddSingleton`'dır (JSON dosyaları ilk kullanımda yüklenir, sonra cache'de tutulur).

### BaseController

Tüm controller'lar `BaseController`'dan türetilir; global olarak `[Authorize]` uygulanır. Anonim erişim için `[AllowAnonymous]` kullanılır. Sağladıkları:

- `T("anahtar")` — `_languageService.Get(CurrentLocale, key)` çağrısının kısayolu
- `AddSuccessMessage(string)` / `AddErrorMessage(string)` — `TempData`'ya yazar
- `ViewBag.Lang` / `ViewBag.CurrentLocale` — view'larda çeviri için
- Akıllı geri navigasyon (`ViewBag.ReturnUrl`) ve breadcrumb üretimi

### DTO ve ViewModel'lar

- Girdi: `CreateXxxDTO` / `UpdateXxxDTO` — action parametreleri, FluentValidation ile doğrulanır (`Validators/` klasörü, otomatik keşfedilir)
- Çıktı: `XxxViewModel` — view modeli, hesaplanmış/join edilmiş alanlar içerebilir
- AutoMapper (`Mappings/MappingProfile.cs`) tüm dönüşümleri yapar; `PasswordHash` ve `IsOffice365User` gibi güvenlik hassas alanlar açıkça `Ignore()` edilir

### Repository Kuralları

Okuma metodlarında (`GetAllAsync`, `FindAsync`, `GetPagedAsync`) varsayılan olarak `AsNoTracking()` kullanılır. Servis katmanında özel sorgular için `Query()` / `QueryAsNoTracking()` ile `.Where()`, `.Include()`, `.ToListAsync()` zincirlenir. `UpdateAsync` açıkça `EntityState.Modified` set eder.

## Çok Kiracılılık (Multi-Tenancy)

Şirket tabanlı kiracılık. Tüm veriler (departmanlar, layout'lar, sayfalar, içerikler, programlar) bir `Company`'ye aittir. Kullanıcılar `UserCompanyRole` ve `UserDepartmentRole` ara tablolarıyla şirket ve departmanlara bağlanır.

**Kiracı çözümleme:** `TenantResolverMiddleware` → `TenantContext` servisi. `HttpContext.Items["CompanyId"]` veya session `SelectedCompanyId`'den okur. Şirket değiştirme: `AccountController.SwitchCompany` session'ı günceller.

**Veri izolasyonu servis/controller katmanında uygulanır** — controller'lar sorguları `CompanyID`/`DepartmentID` ile açıkça filtreler. EF global query filter'ı yoktur.

**Login sonrası context başlatma:** SystemAdmin → `null` context (tüm şirketler). Tek şirket/departman → otomatik seçilir. Birden fazla → "Tümü" modu (`null`). Detaylı senaryolar ve erişim bazlı dropdown davranışı için `.claude/FINAL_CONTEXT_SUMMARY.md`'ye bakınız.

## Yetkilendirme

`Services/AuthorizationService.cs` içinde uygulanan üç kademeli hiyerarşi:

| Kademe | Kontrol | Erişim |
|---|---|---|
| `SystemAdmin` | `User.IsSystemAdmin` flag + claim | Her şey |
| `CompanyAdmin` | `UserCompanyRole.Role == "CompanyAdmin"` | Şirketin tamamı |
| Departman rolleri | `UserDepartmentRole.Role` (Manager/Editor/Viewer) | Yalnızca atanan departmanlar |

Controller'lar ASP.NET policy'lerine tamamen güvenmek yerine `_authService.IsSystemAdminAsync(userId)`, `IsCompanyAdminAsync(userId, companyId)`, `CanAccessDepartmentAsync(userId, departmentId)` metodlarını doğrudan çağırır.

**İzin cache'leme:** `IMemoryCache`; `user_sysadmin_{userId}` anahtarı (15 dk) ve `user_{userId}_company_{companyId}_access` anahtarı (10 dk). Rol değişikliklerinde `ClearUserCache(userId)` çağrılır. Not: yalnızca SystemAdmin anahtarı güvenilir biçimde temizlenir; şirket/departman cache'leri TTL ile expire olur.

**ASP.NET policy'leri** `Program.cs`'de tanımlıdır: `"SystemAdmin"`, `"CompanyAdmin"`, `"Manager"` (Manager/CompanyAdmin/SystemAdmin kabul eder).

**UserController güvenlik korumaları:** Kullanıcılar kendini silemez/devre dışı bırakamaz, kendi `IsSystemAdmin` flag'ini değiştiremez. Son aktif SystemAdmin silinemez/devre dışı bırakılamaz. SystemAdmin kullanıcılara şirket/departman rolü atanamaz. CompanyAdmin, kullanıcıları SystemAdmin yapamaz.

## Kimlik Doğrulama

İki şema:
1. **Cookie (varsayılan):** `/Account/Login` üzerinden yerel giriş. Claim'ler: `NameIdentifier`, `Name`, `Email`, `UserId`, `IsSystemAdmin`, `AuthMethod`. Süre: `AppSettings:CookieExpirationMinutes` (varsayılan 60).
2. **OIDC / Azure AD (opsiyonel):** `ExternalLogin` → Microsoft → `ExternalLoginCallback` yerel cookie üretir. SSO kullanıcıları önceden kayıtlı olmalıdır (`IsOffice365User=true`, `PasswordHash=null`); otomatik kayıt yoktur.

## Yerelleştirme

`wwwroot/lang/en.json`, `tr.json`, `de.json` JSON dosyaları. `LanguageService` (singleton) iç içe JSON'u nokta-notasyonlu anahtarlara düzleştirir (örn. `"nav.dashboard"`). Dil, `locale` cookie'sinden algılanır. Fallback zinciri: istenen dil → İngilizce → anahtarın kendisi. View'larda `ViewBag.Lang.Get(ViewBag.CurrentLocale, "anahtar")` kullanılır.

## Önemli Dosyalar

| Dosya | Amaç |
|---|---|
| `Program.cs` | DI kaydı, middleware pipeline, dev seed |
| `Data/AppDbContext.cs` | Tüm `DbSet<T>`, entity ilişkileri, cascade kuralları, index'ler |
| `Data/IUnitOfWork.cs` / `UnitOfWork.cs` | Repository toplayıcı + transaction yönetimi |
| `Services/AuthorizationService.cs` | Merkezi yetki kontrolleri + cache |
| `Services/TenantContext.cs` | Aktif şirket/departman/kullanıcı context erişimcileri |
| `Middleware/TenantResolverMiddleware.cs` | Her istekte kiracı çözümleme |
| `Controllers/BaseController.cs` | Global auth, çeviri helper, breadcrumb |
| `Mappings/MappingProfile.cs` | Tüm AutoMapper konfigürasyonları |
| `.claude/FINAL_CONTEXT_SUMMARY.md` | Erişim bazlı çok kiracılı context tasarım dokümanı |
