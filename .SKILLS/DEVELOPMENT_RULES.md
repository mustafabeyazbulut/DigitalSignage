# Development Rules & Standards

**Digital Signage v2.0 Profesyonel Geliştirme Kuralları**

---

## 🚨 KRİTİK: SKILLS Dokümantasyon Yönetimi

### ⚠️ ZORUNLU KURALLAR

```
✓ HER ZAMAN .SKILLS/ klasöründeki dokümantasyonu ÖNCE OKUYUN
✓ HER değişiklikten sonra ilgili SKILLS dosyasını GÜNCELLEYIN
✓ Yeni özellik eklerken SKILLS/XX_*.md dosyalarını kontrol edin
✓ Mevcut mimariye UYGUN kod yazın (SKILLS'de belirtildiği gibi)
✗ SKILLS dokümantasyonunu ASLA göz ardı etmeyin
✗ Dokümante edilmemiş değişiklik yapmayın
```

### SKILLS Dosya Yapısı

```
.SKILLS/
├── DEVELOPMENT_RULES.md         ← Bu dosya - Tüm geliştirme kuralları
├── CONTRIBUTING.md              ← Katkı kuralları
├── SKILLS.md                    ← Genel bakış
└── SKILLS/
    ├── 01_ARCHITECTURE.md       ← Mimari yapı
    ├── 02_DATABASE_SCHEMA.md    ← Veritabanı şeması
    ├── 03_DATA_MODELS.md        ← Entity modelleri
    ├── 04_ORM_DATA_ACCESS.md    ← EF Core & Repository
    ├── 05_BUSINESS_LOGIC.md     ← Service katmanı
    ├── 06_MVC_LAYER.md          ← Controller & View
    ├── 07_OFFICE365_AUTH.md     ← Office 365 kimlik doğrulama
    ├── 08_MULTI_TENANT.md       ← Çok kiracılı mimari
    ├── 09_DYNAMIC_LAYOUTS.md    ← Dinamik grid sistemi
    ├── 10_DEPLOYMENT.md         ← Deployment & production
    ├── 11_LOCALIZATION.md       ← Çok dilli destek
    └── 12_AUTHORIZATION.md      ← Yetkilendirme sistemi
```

### Değişiklik Yaparken İzlenecek Adımlar

**1. ÖNCESİ (Kod Yazmadan Önce):**
```bash
# Hangi bölümü değiştireceğinize göre ilgili SKILLS dosyasını okuyun
- Veritabanı değişikliği → 02_DATABASE_SCHEMA.md
- Entity ekleme/değiştirme → 03_DATA_MODELS.md
- Repository ekleme → 04_ORM_DATA_ACCESS.md
- Service mantığı → 05_BUSINESS_LOGIC.md
- Controller/View → 06_MVC_LAYER.md
- Dil paketi → 11_LOCALIZATION.md
- Yetkilendirme → 12_AUTHORIZATION.md
```

**2. SIRASINDA (Kod Yazarken):**
```bash
# SKILLS'de belirtilen kurallara uyun
✓ Naming convention
✓ Folder structure
✓ Pattern kullanımı (Repository, Service, DTO)
✓ Multi-tenant kuralları
✓ Authorization kontrolleri
```

**3. SONRASI (Kod Yazdıktan Sonra):**
```bash
# İlgili SKILLS dosyasını MUTLAKA güncelleyin
✓ Yeni entity eklediyseniz → 02_DATABASE_SCHEMA.md ve 03_DATA_MODELS.md
✓ Yeni service eklediyseniz → 05_BUSINESS_LOGIC.md
✓ Yeni controller eklediyseniz → 06_MVC_LAYER.md
✓ Yeni dil anahtarı eklediyseniz → 11_LOCALIZATION.md
✓ Yeni yetkilendirme kuralı eklediyseniz → 12_AUTHORIZATION.md
✓ README.md'de version number ve "Recent Updates" bölümünü güncelleyin
```

### Örnek Senaryo

```
Senaryo: Email notification özelliği ekliyorsunuz

1. ÖNCE OKU:
   ✓ 03_DATA_MODELS.md → User entity yapısını anla
   ✓ 05_BUSINESS_LOGIC.md → Service pattern'i anla
   ✓ 11_LOCALIZATION.md → Dil paketi yapısını anla

2. KOD YAZ:
   ✓ User modeline EmailNotificationsEnabled ekle
   ✓ Migration oluştur
   ✓ UpdateSettings action'ı ekle
   ✓ Dil paketlerine settings.* anahtarları ekle

3. DÖKÜMAN GÜNCELLE:
   ✓ 03_DATA_MODELS.md → User entity'sine yeni field ekle
   ✓ 11_LOCALIZATION.md → settings.* bölümünü güncelle
   ✓ README.md → v2.2.1 güncelle, "Recent Updates" ekle
```

### ⚡ HIZLI KONTROL LİSTESİ

Kod yazmadan önce kendinize sorun:

- [ ] İlgili SKILLS dosyasını okudum mu?
- [ ] Mevcut pattern'lere uygun kod mu yazıyorum?
- [ ] Bu değişiklik hangi SKILLS dosyalarını etkiliyor?
- [ ] Kod yazdıktan sonra hangi SKILLS dosyalarını güncelleyeceğim?

**UNUTMAYIN:** SKILLS dosyaları projenin "kaynak doğruluğu" (source of truth) dır. Her zaman güncel ve doğru tutulmalıdır!

---

## 1. Dosya & Folder Management

### 1.1 Yeni Dosya Ekleme

```
KURALLAR:
✓ Her .cs dosyası .csproj'ya tanımlanmalı
✓ Klasör yapısı namespace'le match etmeli
✓ Dosya adı PascalCase olmalı
✓ Bir dosya = bir public class (opsiyonel: inner classes OK)
✗ Utils.cs, Helper.cs gibi generic adlar YASAKLANDI
```

### 1.2 .csproj Tanımlaması

```xml
<!-- Controllers -->
<ItemGroup>
  <Compile Include="Controllers/UserController.cs" />
  <Compile Include="Controllers/PageController.cs" />
</ItemGroup>

<!-- Views - Embedded Resource -->
<ItemGroup>
  <EmbeddedResource Include="Views/**/*.cshtml" />
</ItemGroup>

<!-- Services -->
<ItemGroup>
  <Compile Include="Services/IUserService.cs" />
  <Compile Include="Services/UserService.cs" />
</ItemGroup>

<!-- Repositories -->
<ItemGroup>
  <Compile Include="Data/Repositories/IUserRepository.cs" />
  <Compile Include="Data/Repositories/UserRepository.cs" />
</ItemGroup>
```

### 1.3 Namespace Yapısı

```
Dosya: Controllers/User/UserController.cs
Namespace: DigitalSignage.Controllers.User

Dosya: Services/User/UserService.cs
Namespace: DigitalSignage.Services.User

Dosya: Data/Repositories/UserRepository.cs
Namespace: DigitalSignage.Data.Repositories
```

---

## 2. Code Architecture Rules

### 2.1 Layer Separation

```
Controllers
    ↓ (kul kullanıyor)
Services (İş Mantığı)
    ↓ (kullanıyor)
Repositories (Veri Erişimi)
    ↓ (kullanıyor)
DbContext (EF Core)
    ↓ (kullanıyor)
Database
```

**KURALLAR:**
- ✓ Controller → Service → Repository
- ✗ Repository → Service kontrol **YASAKLANDI**
- ✗ Controller'dan direkt DbContext erişimi **YASAKLANDI**
- ✗ View'den direkt Service çağrısı **YASAKLANDI**

### 2.2 Service Layer Kuralları

```csharp
// ✓ DOĞRU
public class PageService : IPageService
{
    private readonly IPageRepository _pageRepository;
    private readonly ILayoutService _layoutService;
    private readonly ILogger<PageService> _logger;

    // Ctor Injection
    public PageService(
        IPageRepository pageRepository,
        ILayoutService layoutService,
        ILogger<PageService> logger)
    {
        _pageRepository = pageRepository;
        _layoutService = layoutService;
        _logger = logger;
    }

    // İş mantığı
    public async Task<Page> CreateWithLayoutAsync(CreatePageDTO dto)
    {
        // Validation
        if (!await ValidatePageAsync(dto))
            throw new ValidationException("Invalid page data");

        // İş işlemi
        var layout = await _layoutService.GetByIdAsync(dto.LayoutID);
        var page = new Page { /* ... */ };

        // Kaydı
        return await _pageRepository.AddAsync(page);
    }
}

// ✗ YASAKLANDI - DbContext doğrudan kullanma
public class PageService
{
    private readonly AppDbContext _context;

    public void CreatePage()
    {
        var page = new Page { };
        _context.Pages.Add(page);  // ✗ YASAKLANDI
        _context.SaveChanges();
    }
}
```

### 2.3 Repository Pattern

```csharp
// ✓ DOĞRU - Generic Repository Interface
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// ✓ DOĞRU - Specialized Repository
public interface IPageRepository : IRepository<Page>
{
    Task<IEnumerable<Page>> GetByDepartmentIdAsync(int deptId);
    Task<Page> GetWithLayoutAsync(int pageId);
}

// ✗ YASAKLANDI - SQL doğrudan yazma
public class PageRepository
{
    public void CreatePage(Page page)
    {
        var sql = "INSERT INTO Pages VALUES (...)";  // ✗ YASAKLANDI
        _context.Database.ExecuteSqlRaw(sql);
    }
}
```

---

## 3. Entity Framework Core Rules

### 3.1 DbContext Konfigürasyonu

```csharp
// ✓ DOĞRU
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Page> Pages { get; set; }
    public DbSet<Content> Contents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Fluent API ile konfigürasyonlar
        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasKey(e => e.PageID);
            entity.Property(e => e.PageName).IsRequired().HasMaxLength(255);
            entity.HasMany(e => e.PageContents)
                .WithOne(pc => pc.Page)
                .HasForeignKey(pc => pc.PageID)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

// ✗ YASAKLANDI - Data Annotations kullanma
public class Page
{
    [Key]
    public int PageID { get; set; }

    [Required]
    [MaxLength(255)]
    public string PageName { get; set; }  // ✗ Fluent API kullan
}
```

### 3.2 Queries

```csharp
// ✓ DOĞRU - Include kullanarak N+1 problem çözümleme
var pages = await _context.Pages
    .Include(p => p.Layout)
        .ThenInclude(l => l.LayoutSections)
    .Include(p => p.PageContents)
        .ThenInclude(pc => pc.Content)
    .Where(p => p.DepartmentID == deptId && p.IsActive)
    .ToListAsync();

// ✓ DOĞRU - AsNoTracking kullanarak read-only sorguları optimize etme
var layouts = await _context.Layouts
    .AsNoTracking()
    .Where(l => l.CompanyID == companyId)
    .ToListAsync();

// ✗ YASAKLANDI - N+1 sorgusu
foreach (var page in pages)
{
    var layout = await _context.Layouts
        .FirstOrDefaultAsync(l => l.LayoutID == page.LayoutID);  // ✗ Loop içinde
}

// ✗ YASAKLANDI - Lazy Loading (explicit Include kullan)
var page = await _context.Pages.FirstOrDefaultAsync();
var layout = page.Layout;  // ✗ Lazy loading, mapping'te null olabilir
```

### 3.3 Migrations

```bash
# ✓ DOĞRU
dotnet ef migrations add AddPageContentTable --context AppDbContext
dotnet ef migrations add RemoveSystemUnitEntity --context AppDbContext

# ✗ YASAKLANDI - Eski migration'ları silme (git'te history kalır)
dotnet ef migrations remove  # SADECE latest henüz prod'a çıkmadıysa

# ✓ Migration isimlendir
# Format: [Date]_[ActionDescription].cs
# Örnek: 20250209_AddCompanyConfigurationTable.cs
```

---

## 4. Multi-Tenant Rules

### 4.1 Tenant Context

```csharp
// ✓ HER QUERY'DE TENANT CHECK
var pages = await _repository.FindAsync(p =>
    p.Department.CompanyID == _tenantContext.CurrentCompanyId
);

// ✗ YASAKLANDI - Tenant check olmadan query
var pages = await _repository.GetAllAsync();  // ✗ Diğer şirketlerin verisini gösterebilir

// ✓ Service layer'da tenant check
public class PageService
{
    public async Task<Page> GetByIdAsync(int pageId)
    {
        var page = await _pageRepository.GetByIdAsync(pageId);

        if (page?.Department.CompanyID != _tenantContext.CurrentCompanyId)
            throw new UnauthorizedAccessException("Access denied");

        return page;
    }
}
```

### 4.2 Per-Company Configuration

```csharp
// ✓ DOĞRU - Configuration servisi kullan
public class PageService
{
    public async Task<Page> CreateWithConfigAsync(CreatePageDTO dto)
    {
        var config = await _configService.GetConfigAsync(_tenantContext.CurrentCompanyId);

        var page = new Page
        {
            /* ... */
            // Config'den varsayılanları kullan
        };

        return await _pageRepository.AddAsync(page);
    }
}

// ✗ YASAKLANDI - Hardcoded values
public class PageService
{
    public async Task<Page> CreatePageAsync(CreatePageDTO dto)
    {
        var page = new Page
        {
            /* ... */
            // Layout sadece 2x2 olabilir  ✗ Hardcoded
        };

        return await _pageRepository.AddAsync(page);
    }
}
```

---

## 5. Security Rules

### 5.1 Input Validation

```csharp
// ✓ DOĞRU - Validation dto kullan
[HttpPost]
public async Task<IActionResult> CreatePage([FromBody] CreatePageDTO dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var page = await _pageService.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = page.PageID }, page);
}

// ✗ YASAKLANDI - Doğrudan user input'u kullanma
[HttpPost]
public async Task<IActionResult> CreatePage(string name, string description)
{
    var page = new Page { PageName = name, Description = description };
    await _context.Pages.AddAsync(page);  // ✗ Validation yok
}
```

### 5.2 SQL Injection Prevention

```csharp
// ✓ DOĞRU - Parametreli queries
var pages = await _context.Pages
    .FromSqlInterpolated($"SELECT * FROM Pages WHERE CompanyID = {companyId}")
    .ToListAsync();

// ✗ YASAKLANDI - String concatenation
var sql = $"SELECT * FROM Pages WHERE CompanyID = {companyId}";  // ✗ SQL Injection
var pages = await _context.Pages.FromSqlRaw(sql).ToListAsync();
```

### 5.3 Authorization

```csharp
// ✓ DOĞRU - Role check'i
[Authorize(Roles = "CompanyAdmin")]
[HttpPost("{companyId}/configure")]
public async Task<IActionResult> UpdateConfiguration(int companyId, [FromBody] CompanyConfigurationDTO dto)
{
    if (!await _tenantContext.IsCompanyAdminAsync(companyId))
        return Forbid();

    // Implementation
}

// ✗ YASAKLANDI - Authorization olmadan sensitive operation
[HttpPost("{companyId}/delete")]
public async Task<IActionResult> DeleteCompany(int companyId)
{
    // No authorization check ✗
    await _companyService.DeleteAsync(companyId);
}
```

---

## 6. Async/Await Rules

### 6.1 Async Best Practices

```csharp
// ✓ DOĞRU
public async Task<Page> GetPageAsync(int id)
{
    return await _pageRepository.GetByIdAsync(id);
}

// ✗ YASAKLANDI - Sync over async (deadlock risk)
public Page GetPage(int id)
{
    return _pageRepository.GetByIdAsync(id).Result;  // ✗ YASAKLANDI
}

// ✗ YASAKLANDI - async void (exception handling impossible)
public async void LoadPages()  // ✗ YASAKLANDI
{
    var pages = await _pageService.GetAllAsync();
}

// ✓ DOĞRU - Task dön
public async Task LoadPagesAsync()
{
    var pages = await _pageService.GetAllAsync();
}
```

### 6.2 Cancellation Tokens

```csharp
// ✓ DOĞRU - Cancellation token kullan
public async Task<Page> GetByIdAsync(int id, CancellationToken cancellationToken = default)
{
    return await _pageRepository.GetByIdAsync(id, cancellationToken);
}

// Controller'da
[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
{
    var page = await _pageService.GetByIdAsync(id, cancellationToken);
    return Ok(page);
}
```

---

## 7. Testing Rules

### 7.1 Unit Test Standards

```csharp
// ✓ DOĞRU - AAA Pattern (Arrange, Act, Assert)
[Fact]
public async Task CreatePageAsync_WithValidData_ReturnsPage()
{
    // Arrange
    var dto = new CreatePageDTO { PageName = "Test", DepartmentID = 1 };
    _mockRepository.Setup(r => r.AddAsync(It.IsAny<Page>()))
        .ReturnsAsync((Page p) => { p.PageID = 1; return p; });

    // Act
    var result = await _pageService.CreateAsync(dto);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.PageID);
    _mockRepository.Verify(r => r.AddAsync(It.IsAny<Page>()), Times.Once);
}

// ✗ YASAKLANDI - Gerçek database kullanma
[Fact]
public void CreatePage_SavesToDatabase()
{
    var page = new Page { PageName = "Test" };
    _context.Pages.Add(page);  // ✗ Real DB, test isolation yok
    _context.SaveChanges();
}
```

### 7.2 Test Coverage

```
Coverage Requirements:
- Services: ≥ 80%
- Repositories: ≥ 70%
- Helpers: ≥ 60%
- Critical paths: 100%

YASAKLANDI:
✗ Sadece happy path test'leri
✗ Exception handling test'leri olmadan
✗ Edge case'ler test edilmemiş
```

---

## 8. Performance Rules

### 8.1 Caching

```csharp
// ✓ DOĞRU - Configuration cache
public async Task<CompanyConfiguration> GetConfigAsync(int companyId)
{
    var cacheKey = $"company_config_{companyId}";

    if (_cache.TryGetValue(cacheKey, out CompanyConfiguration config))
        return config;

    config = await _repository.GetAsync(companyId);
    _cache.Set(cacheKey, config, TimeSpan.FromHours(1));

    return config;
}

// ✗ YASAKLANDI - Cache invalidation olmadan long-term cache
_cache.Set(cacheKey, data);  // ✗ Hiç expire etmiyor
```

### 8.2 Pagination

```csharp
// ✓ DOĞRU - Pagination kullan
[HttpGet]
public async Task<IActionResult> GetPages(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
{
    const int maxPageSize = 100;
    if (pageSize > maxPageSize)
        pageSize = maxPageSize;

    var pages = await _pageService.GetPagedAsync(pageNumber, pageSize);
    return Ok(pages);
}

// ✗ YASAKLANDI - Tüm records'ı fetch etme
var pages = await _pageService.GetAllAsync();  // ✗ Büyük veri = crash
```

---

## 9. Documentation Rules

### 9.1 XML Comments

```csharp
// ✓ DOĞRU
/// <summary>
/// Sayfa ID'sine göre sayfayı getirir.
/// </summary>
/// <param name="pageId">Sayfa ID'si</param>
/// <returns>Page entity'si veya null</returns>
/// <exception cref="UnauthorizedAccessException">Erişim hakkı yoksa</exception>
public async Task<Page> GetByIdAsync(int pageId)
{
    // Implementation
}

// ✗ YASAKLANDI - Yorum olmadan public method
public async Task<Page> GetById(int pageId)  // ✗ Dokümantasyon yok
{
    // Implementation
}
```

### 9.2 README Updates

```
Kural:
✓ Yeni public API'lar README'ye eklenmeli
✓ Breaking change'ler dokümante edilmeli
✓ Setup instructions güncellenmiş olmalı
```

---

## 10. Checklist - PR Gönderme Öncesi

- [ ] `.csproj` güncellenmiş mi? (yeni dosyalar)
- [ ] Namespace'ler doğru mu?
- [ ] Tüm public method'lar XML doc'lu mu?
- [ ] Tenant check'i var mı? (multi-tenant ops)
- [ ] Input validation var mı?
- [ ] Unit test yazıldı mı? (≥ 80% coverage)
- [ ] `dotnet build` başarılı mı?
- [ ] `dotnet test` geçti mi?
- [ ] Database migration'lar var mı? (yeni entities)
- [ ] Code style tutarlı mı?

---

**Son güncelleme:** 12 Şubat 2026
