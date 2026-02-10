# Sistem Mimarisi

## Genel Bakış

Digital Signage, N-tier mimarisine dayanan bir ASP.NET Core 9 MVC uygulamasıdır. Sistem, kurumsal dijital işaretleme cihazlarının merkezi yönetimini sağlar.

## Mimari Katmanlar

```
┌─────────────────────────────────────┐
│   Presentation Layer (UI)           │
│  (Views, Controllers, Razor Pages)  │
├─────────────────────────────────────┤
│   Application Logic Layer           │
│  (Services, Business Logic, DTOs)   │
├─────────────────────────────────────┤
│   Data Access Layer                 │
│  (Repository Pattern, EF Core)      │
├─────────────────────────────────────┤
│   Database Layer                    │
│  (SQL Server 2022+)                 │
└─────────────────────────────────────┘
```

### 1. Presentation Layer (Sunum Katmanı)
- **Controllers**: HTTP isteklerini işleyin
- **Views**: Razor templates ile HTML render
- **ViewModels**: View-specific veri modelleri
- **Tag Helpers**: Reusable UI components
- **Routing**: Clean URLs ve REST conventions

### 2. Application Logic Layer (İş Mantığı Katmanı)
- **Services**: Business logic encapsulation
- **Managers**: Complex operations
- **Validators**: Data validation rules
- **DTOs**: Data Transfer Objects
- **Dependency Injection**: Loose coupling

### 3. Data Access Layer (Veri Erişim Katmanı)
- **Repository Pattern**: Data abstraction
- **DbContext**: EF Core context
- **Entity Configurations**: Fluent API
- **Migrations**: Database versioning
- **Unit of Work**: Transaction management

### 4. Database Layer (Veritabanı Katmanı)
- **SQL Server 2022+**: Relational database
- **Stored Procedures**: Complex queries
- **Indexes**: Performance optimization
- **Constraints**: Data integrity

## Bileşen Diyagramı

```
┌─────────────────────────────────────────────────────┐
│                  Web Browser / Client               │
└──────────────┬──────────────────────────────────────┘
               │
┌──────────────▼──────────────────────────────────────┐
│              ASP.NET Core 9 MVC Application         │
│  ┌────────────────────────────────────────────────┐ │
│  │ Controllers (HomeController, UserController)  │ │
│  │ Views (*.cshtml) & ViewModels                  │ │
│  └────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────┐ │
│  │ Services & Managers (Business Logic)           │ │
│  │ Validators, Helpers, Mappers                   │ │
│  └────────────────────────────────────────────────┘ │
│  ┌────────────────────────────────────────────────┐ │
│  │ Repository Pattern (Data Access)               │ │
│  │ Entity Framework Core 9                        │ │
│  └────────────────────────────────────────────────┘ │
└──────────────┬──────────────────────────────────────┘
               │
┌──────────────▼──────────────────────────────────────┐
│            SQL Server 2022+ Database                │
│  (Tables, Views, Stored Procedures, Indexes)       │
└──────────────────────────────────────────────────────┘
```

## Veri Akışı

### Create/Insert Flow
```
User Input (Form)
    ↓
Controller.Post(ViewModel)
    ↓
Service.Validate(Data)
    ↓
Service.Create(DTO)
    ↓
Repository.Add(Entity)
    ↓
DbContext.SaveChanges()
    ↓
Database (INSERT)
    ↓
Redirect / Response
```

### Read/Fetch Flow
```
User Request (URL)
    ↓
Controller.Get(Id)
    ↓
Repository.GetById(Id)
    ↓
DbContext.Query()
    ↓
Database (SELECT)
    ↓
Entity → DTO → ViewModel
    ↓
View (Render HTML)
    ↓
Browser Response
```

### Update Flow
```
User Input (Edit Form)
    ↓
Controller.Post(ViewModel)
    ↓
Service.Validate(Data)
    ↓
Repository.GetById(Id)
    ↓
Entity.Update(Properties)
    ↓
DbContext.SaveChanges()
    ↓
Database (UPDATE)
    ↓
Redirect / Response
```

### Delete Flow
```
User Action (Delete Button)
    ↓
Controller.Delete(Id)
    ↓
Service.CheckPermission(User)
    ↓
Repository.Delete(Id)
    ↓
DbContext.SaveChanges()
    ↓
Database (DELETE)
    ↓
Redirect / Response
```

## Proje Klasör Yapısı

```
DigitalSignage/
├── Controllers/                    # MVC Controllers
│   ├── HomeController.cs
│   ├── UserController.cs
│   ├── DepartmentController.cs
│   ├── PageController.cs
│   ├── LayoutController.cs
│   ├── ContentController.cs
│   ├── ScheduleController.cs
│   └── CompanyController.cs        # NEW
├── Views/                          # Razor Templates
│   ├── Home/
│   ├── User/
│   ├── Department/
│   ├── Page/
│   ├── Layout/
│   ├── Content/
│   ├── Schedule/
│   └── Company/                    # NEW
├── Models/                         # Entity Models
│   ├── User.cs
│   ├── Company.cs                  # NEW
│   ├── SystemUnit.cs               # NEW
│   ├── Department.cs
│   ├── Page.cs
│   ├── Layout.cs
│   ├── LayoutSection.cs
│   ├── Content.cs
│   ├── Schedule.cs
│   └── SchedulePage.cs
├── ViewModels/                     # View-specific Models
│   ├── UserViewModel.cs
│   ├── CompanyViewModel.cs         # NEW
│   ├── SystemUnitViewModel.cs      # NEW
│   ├── DepartmentViewModel.cs
│   ├── PageViewModel.cs
│   ├── LayoutViewModel.cs
│   ├── DynamicLayoutViewModel.cs   # NEW
│   └── ...
├── Data/                           # Data Access Layer
│   ├── AppDbContext.cs             # EF Core Context
│   ├── Repositories/
│   │   ├── IRepository.cs          # Interface
│   │   ├── Repository.cs           # Base Class
│   │   ├── UserRepository.cs
│   │   ├── DepartmentRepository.cs
│   │   ├── PageRepository.cs
│   │   ├── CompanyRepository.cs    # NEW
│   │   ├── SystemRepository.cs     # NEW
│   │   └── ...
│   └── Migrations/                 # EF Migrations
│       ├── 001_InitialCreate.cs
│       ├── 002_AddCompanySystem.cs # NEW
│       └── ...
├── Services/                       # Business Logic
│   ├── IService.cs                 # Interface
│   ├── UserService.cs
│   ├── CompanyService.cs           # NEW
│   ├── SystemService.cs            # NEW
│   ├── DepartmentService.cs
│   ├── PageService.cs
│   ├── LayoutService.cs
│   ├── ContentService.cs
│   ├── ScheduleService.cs
│   └── ...
├── Helpers/                        # Utility Classes
│   ├── AuthHelper.cs
│   ├── ValidationHelper.cs
│   ├── MappingHelper.cs
│   ├── PermissionHelper.cs         # NEW
│   └── ...
├── Filters/                        # Action Filters
│   ├── AuthorizeFilter.cs
│   ├── ExceptionFilter.cs
│   └── ...
├── appsettings.json                # Configuration
├── appsettings.Development.json
├── appsettings.Production.json
├── Program.cs                      # Startup/DI Configuration
└── ...
```

## Bağımlılık Yönetimi (Dependency Injection)

### IoC Container Configuration (Program.cs)
```
Services.AddScoped<IRepository<T>, Repository<T>>()
Services.AddScoped<IUserService, UserService>()
Services.AddScoped<ICompanyService, CompanyService>()
Services.AddScoped<ISystemService, SystemService>()
Services.AddScoped<ILayoutService, LayoutService>()
...
```

### Constructor Injection Pattern
```csharp
public class UserController : Controller
{
    private readonly IUserService _userService;
    private readonly ICompanyService _companyService;

    public UserController(
        IUserService userService,
        ICompanyService companyService)
    {
        _userService = userService;
        _companyService = companyService;
    }
}
```

## Güvenlik Mimarisi

### Authentication
- Identity Framework
- Cookie-based authentication
- Password hashing (bcrypt/PBKDF2)

### Authorization
- Role-Based Access Control (RBAC)
- Claims-based authorization
- Policy-based authorization

### Data Protection
- SQL Injection Prevention (Parameterized Queries)
- CSRF Token Protection
- XSS Protection (HTML Encoding)
- CORS Configuration

## Performance Considerations

### Caching
- In-Memory Caching
- Distributed Cache (Redis optional)

### Database Optimization
- Proper Indexing
- Query Optimization (Select N+1 prevention)
- Connection Pooling

### Load Balancing
- Stateless Design
- Session Management via Database

---

**Referanslar**:
- ASP.NET Core Architecture: https://docs.microsoft.com/aspnet
- EF Core Documentation: https://docs.microsoft.com/ef
- Repository Pattern: https://martinfowler.com/eaaCatalog/repository.html
