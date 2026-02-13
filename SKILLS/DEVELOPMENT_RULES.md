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

### 8.2 Server-Side Filtreleme, Sıralama ve Sayfalama (ZORUNLU)

**KURAL: FİLTRELEME, SIRALAMA VE SAYFALAMA SUNUCU TARAFINDA YAPILMALIDIR**

Tüm liste sayfaları (Index view'ları) için filtreleme, sıralama ve sayfalama işlemleri server-side yapılmalıdır. Client-side yaklaşım tüm verileri yükler ve bu performans sorunu yaratır.

#### ✅ DOĞRU - Server-Side Yaklaşım

```csharp
// Controller - Server-side işlemler
[HttpGet]
public async Task<IActionResult> Index(string search = "", string sortBy = "", string sortOrder = "asc", int page = 1)
{
    const int pageSize = 10;

    // Tüm kullanıcıları al
    var allUsers = await _userService.GetAllAsync();
    IEnumerable<User> query = allUsers;

    // ✓ Server-side arama filtresi
    if (!string.IsNullOrEmpty(search))
    {
        search = search.ToLower();
        query = query.Where(u =>
            (u.UserName != null && u.UserName.ToLower().Contains(search)) ||
            (u.Email != null && u.Email.ToLower().Contains(search))
        );
    }

    // ✓ Server-side sıralama
    query = sortBy switch
    {
        "UserName" => sortOrder == "asc"
            ? query.OrderBy(u => u.UserName)
            : query.OrderByDescending(u => u.UserName),
        "Email" => sortOrder == "asc"
            ? query.OrderBy(u => u.Email)
            : query.OrderByDescending(u => u.Email),
        _ => query.OrderBy(u => u.UserName)
    };

    // ✓ Server-side pagination
    var totalCount = query.Count();
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    var users = query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    // ViewBag ile view'a parametre gönder
    ViewBag.CurrentPage = page;
    ViewBag.TotalPages = totalPages;
    ViewBag.SearchTerm = search;
    ViewBag.SortBy = sortBy;
    ViewBag.SortOrder = sortOrder;

    return View(users);
}
```

```html
<!-- View - GET form ile arama -->
<form method="get" asp-action="Index" class="row g-3">
    <div class="col-md-10">
        <input type="text" name="search" class="form-control"
               placeholder="Ara..." value="@ViewBag.SearchTerm">
    </div>
    <div class="col-md-2">
        <button type="submit" class="btn btn-primary w-100">Ara</button>
    </div>
</form>

<!-- Sıralanabilir tablo başlığı -->
<th>
    <a href="?search=@ViewBag.SearchTerm&sortBy=UserName&sortOrder=@(ViewBag.SortBy == "UserName" && ViewBag.SortOrder == "asc" ? "desc" : "asc")">
        Kullanıcı Adı
        @if (ViewBag.SortBy == "UserName")
        {
            <i class="bi bi-arrow-@(ViewBag.SortOrder == "asc" ? "up" : "down")"></i>
        }
    </a>
</th>

<!-- Pagination -->
<nav>
    <ul class="pagination">
        <li class="page-item @(ViewBag.CurrentPage == 1 ? "disabled" : "")">
            <a class="page-link" href="?search=@ViewBag.SearchTerm&sortBy=@ViewBag.SortBy&sortOrder=@ViewBag.SortOrder&page=@(ViewBag.CurrentPage - 1)">
                Önceki
            </a>
        </li>
        @for (int i = 1; i <= ViewBag.TotalPages; i++)
        {
            <li class="page-item @(i == ViewBag.CurrentPage ? "active" : "")">
                <a class="page-link" href="?search=@ViewBag.SearchTerm&sortBy=@ViewBag.SortBy&sortOrder=@ViewBag.SortOrder&page=@i">@i</a>
            </li>
        }
        <li class="page-item @(ViewBag.CurrentPage == ViewBag.TotalPages ? "disabled" : "")">
            <a class="page-link" href="?search=@ViewBag.SearchTerm&sortBy=@ViewBag.SortBy&sortOrder=@ViewBag.SortOrder&page=@(ViewBag.CurrentPage + 1)">
                Sonraki
            </a>
        </li>
    </ul>
</nav>
```

#### ❌ YASAKLANDI - Client-Side Yaklaşım

```javascript
// ✗ JavaScript ile client-side filtreleme - YASAKLANDI
const TableFeatures = {
    init: function(tableId) {
        // ✗ Tüm veriler yükleniyor - performans sorunu
        const allRows = table.querySelectorAll('tbody tr');

        // ✗ JavaScript ile filtreleme
        searchInput.addEventListener('input', (e) => {
            allRows.forEach(row => {
                if (row.textContent.includes(e.target.value)) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        });
    }
};
```

```html
<!-- ✗ YASAKLANDI - Client-side için tüm veriler HTML'de -->
<table id="users-table">
    <tbody>
        @foreach (var user in Model)  <!-- ✗ Tüm 10,000 kullanıcı yükleniyor -->
        {
            <tr>
                <td>@user.UserName</td>
                <td>@user.Email</td>
            </tr>
        }
    </tbody>
</table>

<script>
    // ✗ YASAKLANDI - Client-side table features
    TableFeatures.init('#users-table');
</script>
```

#### 📋 Server-Side Avantajları

1. **Performans**: Sadece gerekli veri yüklenir (örn: 10 kayıt), tümü değil (10,000 kayıt)
2. **Network**: Daha az veri transfer edilir
3. **Memory**: Browser memory'sinde tüm veri tutulmaz
4. **Scalability**: Büyük veri setleriyle çalışabilir
5. **SEO**: Query string ile URL paylaşılabilir (`?search=test&page=2`)

#### ⚠️ Bu Kural Neden Önemli?

```
Client-Side Yaklaşım:
❌ Tüm 10,000 kullanıcı → HTML (5MB) → Browser
❌ JavaScript filtreleme → Tüm 10,000 kayıt memory'de
❌ Sayfa yüklenirken 5MB transfer
❌ Mobile'da crash riski yüksek

Server-Side Yaklaşım:
✅ Sadece 10 kullanıcı → HTML (50KB) → Browser
✅ Database filtreleme → Optimize edilmiş query
✅ Sayfa yüklenirken 50KB transfer
✅ Mobile'da sorunsuz çalışır
```

#### 🚨 İhlal Durumında

```
PR Review sürecinde:
1. Client-side filtreleme/pagination tespit edilirse → PR rejected
2. Kod server-side yaklaşıma dönüştürülür
3. JavaScript table features kaldırılır
4. GET form ve query string parametreleri eklenir
5. Review tekrarlanır
6. Onaylandıktan sonra merge edilir
```

**SONUÇ: Tüm filtreleme, sıralama ve sayfalama işlemleri server-side yapılmalıdır. Client-side yaklaşım yasaklanmıştır.**

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

### 9.3 🇹🇷 Kod Açıklama Dili Kuralı (ZORUNLU)

**KURAL: TÜM KOD AÇIKLAMALARI TÜRKÇE OLMALIDIR**

Bu proje Türk geliştirme ekibi için tasarlanmıştır. Kod okunabilirliği ve bakım kolaylığı için:

#### ✅ DOĞRU Kullanım:

```csharp
// ✓ XML Documentation Comments - TÜRKÇE
/// <summary>
/// Kullanıcıyı email adresine göre getirir.
/// </summary>
/// <param name="email">Kullanıcının email adresi</param>
/// <returns>Kullanıcı entity'si veya null</returns>
public async Task<User?> GetByEmailAsync(string email)
{
    // ✓ Inline comments - TÜRKÇE
    // Email'i küçük harfe çevir (case-insensitive arama)
    var normalizedEmail = email.ToLower();

    // ✓ Karmaşık mantık açıklaması - TÜRKÇE
    // Office 365 kullanıcıları için özel işlem:
    // 1. Önce yerel veritabanında ara
    // 2. Bulunamazsa Azure AD'den sync et
    // 3. Cache'e kaydet
    var user = await _repository.GetByEmailAsync(normalizedEmail);

    return user;
}

// ✓ Region comments - TÜRKÇE
#region Şifre Yönetimi

// ✓ TODO comments - TÜRKÇE
// TODO: Şifre karmaşıklığı kontrolü eklenecek
// FIXME: Büyük/küçük harf duyarlılığı sorunu var

#endregion

// ✓ Class/Method üstü açıklamalar - TÜRKÇE
/// <summary>
/// Kullanıcı kimlik doğrulama servisi.
/// Email veya username ile giriş yapılmasını sağlar.
/// </summary>
public class AuthenticationService
{
    // ✓ Private field açıklaması - TÜRKÇE
    // Şifre hash'leme için kullanılan helper
    private readonly IPasswordHelper _passwordHelper;
}
```

#### ❌ YANLIŞ Kullanım:

```csharp
// ✗ İngilizce açıklama - YASAKLANDI
/// <summary>
/// Gets user by email address.  // ✗ İngilizce
/// </summary>
public async Task<User?> GetByEmailAsync(string email)
{
    // ✗ Mixed language - YASAKLANDI
    // Convert email to lowercase  // ✗ İngilizce
    var normalizedEmail = email.ToLower();

    // ✗ İngilizce inline comment
    // Search in database  // ✗ İngilizce
    var user = await _repository.GetByEmailAsync(normalizedEmail);

    return user;
}

// ✗ İngilizce TODO
// TODO: Add password complexity check  // ✗ İngilizce
```

#### 📝 İstisnalar (Türkçe Olmayabilir):

**1. Kod Elemanları (İngilizce Kalmalı):**
```csharp
// ✓ DOĞRU - Kod İngilizce, açıklama Türkçe
public class UserService  // ✓ Class adı İngilizce
{
    /// <summary>
    /// Kullanıcıyı getirir.  // ✓ Açıklama Türkçe
    /// </summary>
    public async Task<User> GetUserAsync()  // ✓ Method adı İngilizce
    {
        var userName = "test";  // ✓ Variable adı İngilizce
        // Kullanıcı adını logla  // ✓ Comment Türkçe
        _logger.LogInformation("User: {UserName}", userName);
    }
}
```

**2. Framework/Library Referansları:**
```csharp
// ✓ DOĞRU - Framework terimleri İngilizce kalabilir
/// <summary>
/// Entity Framework Core kullanarak veritabanı işlemleri yapar.
/// </summary>
// Bu metod IQueryable döner ve lazy loading destekler.
```

**3. Teknik Terimler:**
```csharp
// ✓ DOĞRU - Yaygın teknik terimler İngilizce
// Cache'i temizle
// JWT token oluştur
// Hash değerini kontrol et
// Repository pattern kullanılıyor
```

#### 🎯 Türkçe Karakter Kullanımı:

```csharp
// ✓ DOĞRU - Türkçe karakterler kullanılmalı
// Şifre doğrulama işlemi yapılıyor
// Büyük/küçük harf dönüşümü
// İçerik görüntüleme yetkilendirmesi

// ✗ YANLIŞ - Türkçe karakterler atlanmış
// Sifre dogrulama islemi yapiliyor  // ✗ Türkçe karakter yok
```

#### 📋 Checklist - Kod Review Öncesi:

```
Code Review Checklist:
□ Tüm XML documentation comments Türkçe mi?
□ Tüm inline comments (//) Türkçe mi?
□ Tüm TODO/FIXME notları Türkçe mi?
□ Region açıklamaları Türkçe mi?
□ Türkçe karakterler (ş, ğ, ü, ö, ç, ı) doğru kullanılmış mı?
□ Karmaşık mantık yeterince açıklanmış mı? (Türkçe)
□ Public method'ların tümünde XML doc var mı? (Türkçe)
```

#### 🔍 Örnek Kod Review Senaryosu:

**ÖNCE (❌ Hatalı):**
```csharp
/// <summary>
/// Authenticate user with email and password.  // ✗ İngilizce
/// </summary>
public async Task<User?> AuthenticateAsync(string email, string password)
{
    // Convert to lowercase  // ✗ İngilizce
    var normalizedEmail = email.ToLower();

    // Find user  // ✗ İngilizce
    var user = await _userService.GetByEmailAsync(normalizedEmail);

    // Check password  // ✗ İngilizce
    if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
        return null;

    return user;
}
```

**SONRA (✅ Doğru):**
```csharp
/// <summary>
/// Kullanıcıyı email ve şifre ile doğrular.  // ✓ Türkçe
/// </summary>
/// <param name="email">Kullanıcı email adresi</param>  // ✓ Türkçe
/// <param name="password">Kullanıcı şifresi</param>  // ✓ Türkçe
/// <returns>Doğrulama başarılıysa User, değilse null</returns>  // ✓ Türkçe
public async Task<User?> AuthenticateAsync(string email, string password)
{
    // Email'i küçük harfe çevir (case-insensitive karşılaştırma için)  // ✓ Türkçe
    var normalizedEmail = email.ToLower();

    // Kullanıcıyı veritabanından getir  // ✓ Türkçe
    var user = await _userService.GetByEmailAsync(normalizedEmail);

    // Şifre kontrolü yap  // ✓ Türkçe
    if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
        return null;

    return user;
}
```

#### ⚠️ Bu Kural Neden Önemli?

1. **Ekip İletişimi**: Türk geliştirme ekibi için ana dil Türkçe
2. **Kod Okunabilirliği**: Karmaşık mantık ana dilde daha iyi anlaşılır
3. **Bakım Kolaylığı**: Yeni geliştiriciler kodu daha hızlı anlar
4. **Tutarlılık**: Tüm codebase aynı dil standardını kullanır
5. **Bilgi Transferi**: Teknik bilgi kaybı minimize edilir

#### 🚨 İhlal Durumunda:

```
PR Review sürecinde:
1. İngilizce comment tespit edilirse → PR rejected
2. Geliştirici tüm commentleri Türkçe'ye çevirir
3. Review tekrarlanır
4. Onaylandıktan sonra merge edilir
```

**SONUÇ: Bu proje için TÜM kod açıklamaları TÜRKÇE yazılmalıdır. İstisna yoktur.**

---

### 9.4 🎨 CSS Merkezileştirme ve Inline Style Yasaklama (ZORUNLU)

**KURAL: INLINE CSS KULLANIMI YASAKTIR. HER ZAMAN .CSS DOSYASI KULLANILMALIDIR.**

#### ❌ YANLIŞ - Inline Style Kullanımı

```html
<!-- YASAKLANDI - Inline style attribute -->
<div style="color: red; font-size: 14px;">Hata mesajı</div>

<!-- YASAKLANDI - <style> tag içinde -->
<style>
    .custom-button {
        background-color: #007bff;
        padding: 10px;
    }
</style>

<!-- YASAKLANDI - Tekrar eden inline styles -->
<span style="display: inline-block; width: 30px; height: 30px; background-color: #ff0000;"></span>
<span style="display: inline-block; width: 30px; height: 30px; background-color: #00ff00;"></span>
```

#### ✅ DOĞRU - CSS Dosyası Kullanımı

```css
/* wwwroot/css/site.css veya component-specific.css */
.error-message {
    color: red;
    font-size: 14px;
}

.custom-button {
    background-color: #007bff;
    padding: 10px;
}

.color-preview {
    display: inline-block;
    width: 30px;
    height: 30px;
    border: 1px solid #ccc;
    border-radius: 4px;
}
```

```html
<!-- HTML - Sadece class kullanımı -->
<div class="error-message">Hata mesajı</div>
<button class="custom-button">Gönder</button>
<span class="color-preview" style="background-color: #ff0000;"></span>  <!-- Sadece dinamik değerler için style kullanılabilir -->
```

#### 📋 CSS Merkezileştirme Standartları

**1. Tablo Tasarımları (Her Tablo Aynı Görünüm)**

```css
/* wwwroot/css/tables.css */
.data-table {
    width: 100%;
    margin-bottom: 1rem;
    color: #212529;
    border-collapse: collapse;
}

.data-table thead th {
    vertical-align: bottom;
    border-bottom: 2px solid #dee2e6;
    background-color: #f8f9fa;
    font-weight: 600;
    padding: 12px;
}

.data-table tbody td {
    padding: 12px;
    border-bottom: 1px solid #dee2e6;
}

.data-table tbody tr:hover {
    background-color: #f8f9fa;
}

/* Action buttons group */
.table-actions .btn-group {
    display: flex;
    gap: 4px;
}

.table-actions .btn-sm {
    padding: 4px 8px;
    font-size: 0.875rem;
}
```

**2. Card Tasarımları (Tutarlı Card Layout)**

```css
/* wwwroot/css/cards.css */
.detail-card {
    border: 1px solid #dee2e6;
    border-radius: 8px;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.detail-card .card-header {
    background-color: #f8f9fa;
    border-bottom: 1px solid #dee2e6;
    padding: 1rem 1.25rem;
    font-weight: 600;
}

.detail-card .card-body {
    padding: 1.25rem;
}

.detail-card dl.row dt {
    font-weight: 600;
    color: #495057;
}

.detail-card dl.row dd {
    color: #212529;
}
```

**3. Form Tasarımları (Standart Form Layout)**

```css
/* wwwroot/css/forms.css */
.form-standard .form-label {
    font-weight: 600;
    color: #495057;
    margin-bottom: 0.5rem;
}

.form-standard .form-control {
    border-radius: 4px;
    border: 1px solid #ced4da;
    padding: 0.5rem 0.75rem;
}

.form-standard .form-control:focus {
    border-color: #80bdff;
    box-shadow: 0 0 0 0.2rem rgba(0,123,255,.25);
}

.form-actions {
    display: flex;
    gap: 8px;
    justify-content: flex-end;
    margin-top: 1.5rem;
}
```

#### 🎯 İstisnalar (Sadece Bu Durumlarda Inline Style İzinli)

**1. Dinamik Renkler (Veritabanından gelen)**

```html
<!-- ✓ İzinli - Renk değeri runtime'da belirleniyor -->
<span class="color-preview" style="background-color: @Model.PrimaryColor;"></span>
```

**2. Dinamik Pozisyonlama (Grid sistemi)**

```html
<!-- ✓ İzinli - X/Y koordinatları dinamik -->
<div class="grid-item" style="left: @item.PositionX px; top: @item.PositionY px;">
```

**3. Inline SVG Stilleri (SVG içi)**

```html
<!-- ✓ İzinli - SVG element stilleri -->
<svg>
    <rect style="fill: currentColor;" />
</svg>
```

#### 📁 CSS Dosya Organizasyonu

```
wwwroot/css/
├── site.css              ← Global styles
├── tables.css            ← Tüm tablolar için ortak stiller
├── cards.css             ← Tüm card'lar için ortak stiller
├── forms.css             ← Tüm formlar için ortak stiller
├── buttons.css           ← Button stilleri
├── badges.css            ← Badge ve status stilleri
├── navigation.css        ← Navigation ve breadcrumb
└── components/
    ├── user-card.css     ← User-specific component
    ├── company-card.css  ← Company-specific component
    └── ...
```

#### ✅ Checklist: CSS Kullanımı

**Her View Oluştururken:**
- [ ] Hiçbir `style=""` attribute kullanılmadı mı?
- [ ] `<style>` tag'i kullanılmadı mı?
- [ ] Tüm stiller .css dosyasında tanımlandı mı?
- [ ] Tablolar `data-table` class'ını kullanıyor mu?
- [ ] Card'lar `detail-card` class'ını kullanıyor mu?
- [ ] Formlar `form-standard` class'ını kullanıyor mu?
- [ ] Action buttonlar `btn-group` içinde mi?
- [ ] Dinamik değerler dışında inline style yok mu?

#### ⚠️ Bu Kural Neden Önemli?

1. **Tutarlılık**: Tüm sayfalar aynı görsel standartlara sahip
2. **Bakım Kolaylığı**: Tek bir CSS değişikliği tüm siteyi etkiler
3. **Performans**: CSS dosyaları cache'lenebilir, inline styles cache'lenemez
4. **Responsive Design**: Media query'ler sadece CSS dosyalarında çalışır
5. **Temiz HTML**: HTML sadece yapı için kullanılır, stil ayrılır
6. **Debugging**: Chrome DevTools ile CSS debugging kolay
7. **Merkezileştirme**: Tüm tablolar, formlar, card'lar aynı tasarıma sahip

#### 🚨 İhlal Durumunda:

```
PR Review sürecinde:
1. Inline style tespit edilirse → PR rejected
2. Stiller .css dosyasına taşınır
3. Ortak component'ler varsa merkezileştirilir
4. Review tekrarlanır
5. Onaylandıktan sonra merge edilir
```

**SONUÇ: Inline CSS kullanımı yasaktır. Tüm stiller .css dosyalarında merkezileştirilmelidir.**

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

**Son güncelleme:** 13 Şubat 2026
