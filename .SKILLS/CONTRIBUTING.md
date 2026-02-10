# Contributing to Digital Signage

## İçindekiler
1. [Başlamadan Önce](#başlamadan-önce)
2. [Geliştirme Ortamı](#geliştirme-ortamı)
3. [Dosya Yapısı](#dosya-yapısı)
4. [Kod Standartları](#kod-standartları)
5. [Git Workflow](#git-workflow)
6. [Pull Request Süreci](#pull-request-süreci)
7. [Testing](#testing)

---

## Başlamadan Önce

### Gereksinimler
- .NET 9 SDK veya üzeri
- Visual Studio 2022 veya VS Code
- SQL Server 2022+ (local instance)
- Git
- Node.js 18+ (opsiyonel, frontend tooling için)

### Proje Yapısını Anla
1. [SKILLS.md](./SKILLS.md) - Proje mimarisini oku
2. [SKILLS/02_DATABASE_SCHEMA.md](./SKILLS/02_DATABASE_SCHEMA.md) - Veritabanı şemasını öğren
3. [SKILLS/01_ARCHITECTURE.md](./SKILLS/01_ARCHITECTURE.md) - Katmanlar ve bileşenleri anla

---

## Geliştirme Ortamı

### 1. Repository Klonu

```bash
git clone https://github.com/your-org/digital-signage.git
cd DigitalSignage
```

### 2. Dependencies Yükle

```bash
dotnet restore
```

### 3. Veritabanı Kurulumu

```bash
# Migration'ları uygula
dotnet ef database update --context AppDbContext

# Veya manuel SQL dosyalarını çalıştır
# sqlcmd -S localhost -E -i sql/01_Create_Tables.sql
```

### 4. Projeyi Çalıştır

```bash
dotnet run
# veya
dotnet watch run  # Hot reload için
```

Uygulamaya `https://localhost:5000` adresinden erişin.

---

## Dosya Yapısı

```
DigitalSignage/
├── Controllers/           # MVC Controllers
│   ├── IController.cs     # Interface (opsiyonel)
│   └── {Name}Controller.cs
├── Views/                 # Razor Templates
│   └── {Controller}/
│       └── {Action}.cshtml
├── Models/                # Entity Models
│   └── {Entity}.cs
├── ViewModels/            # View-specific Models
│   └── {Entity}ViewModel.cs
├── Data/                  # Data Access Layer
│   ├── AppDbContext.cs
│   ├── Repositories/
│   │   ├── I{Entity}Repository.cs
│   │   └── {Entity}Repository.cs
│   └── Migrations/
│       └── {Date}_{MigrationName}.cs
├── Services/              # Business Logic
│   ├── I{Entity}Service.cs
│   └── {Entity}Service.cs
├── Helpers/               # Utility Classes
│   └── {Feature}Helper.cs
├── Filters/               # Action Filters
│   └── {Feature}Filter.cs
├── DTOs/                  # Data Transfer Objects
│   └── {Entity}DTO.cs
├── sql/                   # Database Scripts
│   └── {Table}_Create.sql
├── SKILLS/                # Documentation
│   └── *.md
├── appsettings.json
├── Program.cs
└── DigitalSignage.csproj
```

### Kurallar

- **Her yeni dosya `.csproj`'ya tanımlanmalıdır** ⚠️
- Klasörlerde namespace'ler match etmeli
- Views dosyaları Controller folder'ında olmalı
- Repositories `Data/Repositories/` içinde
- Services `Services/` içinde

---

## Kod Standartları

### Naming Conventions

| Öğe | Stil | Örnek |
|-----|------|-------|
| Classes | PascalCase | `UserService`, `PageRepository` |
| Methods | PascalCase | `GetUserById()`, `CreatePage()` |
| Properties | PascalCase | `UserName`, `IsActive` |
| Private fields | camelCase | `_userService`, `_logger` |
| Constants | UPPER_SNAKE_CASE | `MAX_PAGE_SIZE = 100` |
| Local variables | camelCase | `userName`, `isValid` |

### C# Code Style

```csharp
// ✅ Good
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid ID", nameof(id));

        var user = await _userRepository.GetByIdAsync(id);
        return user ?? throw new NotFoundException("User not found");
    }
}

// ❌ Bad
public class userservice
{
    private IUserRepository userRepo;

    public User GetById(int ID)
    {
        return userRepo.GetById(ID);
    }
}
```

### Comments & Documentation

```csharp
/// <summary>
/// Kullanıcıyı ID'ye göre getirir.
/// </summary>
/// <param name="id">Kullanıcı ID'si</param>
/// <returns>Kullanıcı entity'si</returns>
/// <exception cref="NotFoundException">Kullanıcı bulunamadıysa</exception>
public async Task<User> GetByIdAsync(int id)
{
    // Implementation
}
```

### Async/Await

- Tüm I/O işlemleri async olmalı
- Metodlar `Async` suffix'i ile bitmelidir
- `async void` **asla** kullanma (exception handling için)

```csharp
// ✅ Good
public async Task<Page> CreatePageAsync(CreatePageDTO dto)
{
    var page = new Page { /* ... */ };
    await _pageRepository.AddAsync(page);
    return page;
}

// ❌ Bad
public void CreatePage(CreatePageDTO dto)
{
    var page = new Page { /* ... */ };
    _pageRepository.AddAsync(page);
}
```

### Dependency Injection

```csharp
// ✅ Interface tanımla
public interface IPageService
{
    Task<Page> GetByIdAsync(int id);
    Task<Page> CreateAsync(CreatePageDTO dto);
}

// ✅ Implement et
public class PageService : IPageService
{
    private readonly IPageRepository _pageRepository;

    public PageService(IPageRepository pageRepository)
    {
        _pageRepository = pageRepository;
    }

    // Implementation
}

// ✅ Program.cs'de kaydet
builder.Services.AddScoped<IPageService, PageService>();
```

---

## Git Workflow

### Branch Naming

```
feature/{feature-name}      # Yeni özellik
fix/{bug-name}              # Bug fix
refactor/{component-name}   # Refactoring
docs/{topic-name}           # Dokümantasyon
```

### Örnekler

```bash
# Feature branch
git checkout -b feature/add-page-scheduling

# Bug fix branch
git checkout -b fix/correct-grid-calculation

# Refactor branch
git checkout -b refactor/simplify-content-service

# Documentation branch
git checkout -b docs/add-api-guidelines
```

### Commit Messages

Türkçe yazabilirsin ama İngilizce tercih edilir.

```
[TYPE] Brief description

Longer description if needed.

Fixes #123
```

### Commit Types

- `feat:` - Yeni özellik
- `fix:` - Bug fix
- `docs:` - Dokümantasyon
- `refactor:` - Kod yapısı değişikliği
- `test:` - Test ekleme/düzeltme
- `chore:` - Build, dependencies vb.

### Örnekler

```bash
git commit -m "feat: add dynamic grid column configuration"
git commit -m "fix: prevent infinite loop in schedule updates"
git commit -m "docs: update API authentication guide"
```

---

## Pull Request Süreci

### Bir PR Oluşturmadan Önce

1. **Branch'i güncelle**
   ```bash
   git fetch origin
   git rebase origin/main
   ```

2. **Testleri çalıştır**
   ```bash
   dotnet test
   ```

3. **Build'i çalıştır**
   ```bash
   dotnet build
   ```

4. **Kod stilini kontrol et**
   - Visual Studio'da Code Analysis çalıştır

### PR Template

```markdown
## Description
Hangi problemi çözdüğünü veya özelliği açıkla.

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Related Issues
Fixes #(issue number)

## Testing
Nasıl test ettin?

## Checklist
- [ ] Kodumu review ettim
- [ ] Yorum ve dokümantasyon ekledim
- [ ] Yeni warning oluşmadı
- [ ] Testler geçti
- [ ] `.csproj` güncelledim (gerekiyorsa)
```

### PR Reviewers

- Minimum 1 approval gerekli
- Merge'den önce tüm conversation'lar çözülmeli

---

## Testing

### Unit Tests

```bash
dotnet test
```

### Test Klasörü Yapısı

```
Tests/
├── Unit/
│   ├── Services/
│   │   └── UserServiceTests.cs
│   └── Repositories/
│       └── UserRepositoryTests.cs
└── Integration/
    └── Controllers/
        └── UserControllerTests.cs
```

### Test Örneği

```csharp
[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsUser()
{
    // Arrange
    var userId = 1;
    var expectedUser = new User { UserID = userId, UserName = "test" };
    _mockRepository.Setup(r => r.GetByIdAsync(userId))
        .ReturnsAsync(expectedUser);

    // Act
    var result = await _userService.GetByIdAsync(userId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(userId, result.UserID);
}
```

### Test Coverage

- En az %70 code coverage amaç
- Critical path'ler %100 olmalı

---

## .csproj File Management

### Yeni Dosya Eklerken

**ÖNEMLİ:** Her yeni `.cs` dosyası `.csproj`'ya otomatik eklenir, fakat manuel kontrol edin:

```bash
# .csproj dosyasını düzenle
<ItemGroup>
  <Compile Include="NewFolder/NewFile.cs" />
</ItemGroup>
```

### Kontrol Listesi

- [ ] Dosya oluşturdum
- [ ] `.csproj`'da ItemGroup'ta tanımlandı mı?
- [ ] Namespace doğru mu?
- [ ] Build başarılı mı? (`dotnet build`)

---

## Code Review Kuralları

### Reviewer'lar Bak

✅ **Code quality:**
- Naming conventions
- Design patterns
- Performance concerns

✅ **Testing:**
- Test coverage
- Edge cases

✅ **Documentation:**
- Comments ve docstrings
- README güncellemesi

❌ **Style nitpicking** - Otomatik formatter kullan

---

## Resources

- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [ASP.NET Core Best Practices](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

---

**Son güncelleme:** 9 Şubat 2025
