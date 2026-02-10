# Veri Modelleri (Models & ViewModels)

## Genel Bakış

ASP.NET Core MVC'de üç tip model kullanılır:
1. **Entity Models**: Veritabanı tabloları ile eşleşen modeller
2. **ViewModels**: View'lar için özel veri modelleri
3. **DTOs**: Hizmetler arası veri transferi

---

## Entity Models

Entity modeller, veritabanı tablolarını temsil eder ve EF Core tarafından yönetilir.

### User (Kullanıcı Entity)

```csharp
public class User
{
    public int UserID { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }  // Office 365 kullanıcıları için NULL
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemAdmin { get; set; }
    public bool IsOffice365User { get; set; }  // Office 365 entegrasyonu
    public string AzureADObjectId { get; set; }  // Azure AD Object ID
    public string PhotoUrl { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Navigation Properties
    public ICollection<UserCompanyRole> UserCompanyRoles { get; set; }
}
```

### Company (Şirket Entity)

```csharp
public class Company
{
    public int CompanyID { get; set; }
    public string CompanyName { get; set; }
    public string CompanyCode { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string ModifiedBy { get; set; }

    // Navigation Properties
    public ICollection<Department> Departments { get; set; }
    public ICollection<Layout> Layouts { get; set; }
    public ICollection<Content> Contents { get; set; }
    public ICollection<UserCompanyRole> UserCompanyRoles { get; set; }
    public CompanyConfiguration Configuration { get; set; }
}
```

### Department (Departman Entity)

```csharp
public class Department
{
    public int DepartmentID { get; set; }
    public int CompanyID { get; set; }
    public string DepartmentName { get; set; }
    public string DepartmentCode { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }

    // Navigation Properties
    public Company Company { get; set; }
    public ICollection<Page> Pages { get; set; }
    public ICollection<Schedule> Schedules { get; set; }
    public ICollection<Content> Contents { get; set; }
}
```

### Layout (Sayfa Tasarımı Entity)

```csharp
public class Layout
{
    public int LayoutID { get; set; }
    public int CompanyID { get; set; }
    public string LayoutName { get; set; }
    public string LayoutCode { get; set; }
    public int GridColumnsX { get; set; }  // Yatay bölüm sayısı
    public int GridRowsY { get; set; }     // Dikey bölüm sayısı
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation Properties
    public Company Company { get; set; }
    public ICollection<LayoutSection> LayoutSections { get; set; }
    public ICollection<Page> Pages { get; set; }
}
```

### LayoutSection (Düzen Bölümü Entity)

```csharp
public class LayoutSection
{
    public int LayoutSectionID { get; set; }
    public int LayoutID { get; set; }
    public string SectionPosition { get; set; }
    public int ColumnIndex { get; set; }
    public int RowIndex { get; set; }
    public string Width { get; set; }
    public string Height { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public Layout Layout { get; set; }
    public ICollection<PageSection> PageSections { get; set; }
}
```

### Page (Sayfa Entity)

```csharp
public class Page
{
    public int PageID { get; set; }
    public int DepartmentID { get; set; }
    public string PageName { get; set; }
    public string PageTitle { get; set; }
    public string PageCode { get; set; }
    public int LayoutID { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation Properties
    public Department Department { get; set; }
    public Layout Layout { get; set; }
    public ICollection<PageContent> PageContents { get; set; }  // N:N with Content
    public ICollection<PageSection> PageSections { get; set; }
    public ICollection<SchedulePage> SchedulePages { get; set; }
}
```

### Content (İçerik Entity - Reusable)

```csharp
public class Content
{
    public int ContentID { get; set; }
    public int DepartmentID { get; set; }  // İçerik departmana ait
    public string ContentType { get; set; }  // Text, Image, Video, HTML
    public string ContentTitle { get; set; }
    public string ContentData { get; set; }
    public string MediaPath { get; set; }
    public string ThumbnailPath { get; set; }
    public int? DurationSeconds { get; set; }  // Video/Slide süresi
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Navigation Properties
    public Department Department { get; set; }
    public ICollection<PageContent> PageContents { get; set; }  // N:N
}
```

### PageContent (Sayfa-İçerik İlişkisi Entity - N:N)

```csharp
public class PageContent
{
    public int PageContentID { get; set; }
    public int PageID { get; set; }
    public int ContentID { get; set; }
    public int DisplayOrder { get; set; }  // Sayfadaki sıra
    public string DisplaySection { get; set; }  // Hangi section'da gösterilecek
    public bool IsActive { get; set; }
    public DateTime AddedDate { get; set; }

    // Navigation Properties
    public Page Page { get; set; }
    public Content Content { get; set; }
}
```

### PageSection (Sayfa Bölümü Entity)

```csharp
public class PageSection
{
    public int PageSectionID { get; set; }
    public int PageID { get; set; }
    public int LayoutSectionID { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public Page Page { get; set; }
    public LayoutSection LayoutSection { get; set; }
}
```

### Schedule (Çizelge Entity)

```csharp
public class Schedule
{
    public int ScheduleID { get; set; }
    public int DepartmentID { get; set; }
    public string ScheduleName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
    public string RecurrencePattern { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation Properties
    public Department Department { get; set; }
    public ICollection<SchedulePage> SchedulePages { get; set; }
}
```

### SchedulePage (Çizelge Sayfası Entity)

```csharp
public class SchedulePage
{
    public int SchedulePageID { get; set; }
    public int ScheduleID { get; set; }
    public int PageID { get; set; }
    public int DisplayDuration { get; set; }  // Saniye
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation Properties
    public Schedule Schedule { get; set; }
    public Page Page { get; set; }
}
```

### UserCompanyRole (Kullanıcı-Şirket Rolü Entity)

```csharp
public class UserCompanyRole
{
    public int UserCompanyRoleID { get; set; }
    public int UserID { get; set; }
    public int CompanyID { get; set; }
    public string Role { get; set; }  // SystemAdmin, CompanyAdmin, Manager, Editor, Viewer
    public bool IsActive { get; set; }
    public DateTime AssignedDate { get; set; }
    public string AssignedBy { get; set; }

    // Navigation Properties
    public User User { get; set; }
    public Company Company { get; set; }
}
```

### CompanyConfiguration (Şirket Konfigürasyonu Entity)

```csharp
public class CompanyConfiguration
{
    public int ConfigurationID { get; set; }
    public int CompanyID { get; set; }

    // Email Settings
    public string EmailSmtpServer { get; set; }
    public int EmailSmtpPort { get; set; }
    public string EmailFrom { get; set; }
    public string EmailUsername { get; set; }
    public string EmailPassword { get; set; }  // Encrypted
    public bool EmailNotificationsEnabled { get; set; }

    // Notification Settings
    public bool NotifyOnScheduleChange { get; set; }
    public bool NotifyOnContentChange { get; set; }
    public bool NotifyOnError { get; set; }

    // Layout Defaults
    public int DefaultGridColumnsX { get; set; } = 2;
    public int DefaultGridRowsY { get; set; } = 2;
    public string DefaultSectionPadding { get; set; } = "10px";

    // Schedule Rules
    public int MaxSchedulesPerPage { get; set; } = 10;
    public int DefaultScheduleDuration { get; set; } = 30;
    public bool AllowRecurringSchedules { get; set; } = true;

    // Display Settings
    public int ScreenRefreshInterval { get; set; } = 5;
    public bool EnableAutoRotation { get; set; } = true;
    public string CustomCSS { get; set; }

    // Feature Flags
    public bool EnableAnalytics { get; set; } = true;
    public bool EnableAdvancedScheduling { get; set; } = true;
    public bool EnableMediaUpload { get; set; } = true;
    public int MaxMediaSizeGB { get; set; } = 10;

    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Navigation Properties
    public Company Company { get; set; }
}
```

---

## ViewModels

ViewModels, View'lar tarafından kullanılmak üzere özel olarak tasarlanmış veri modelleridir.

### UserViewModel

```csharp
public class UserViewModel
{
    public int UserID { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystemAdmin { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }

    // Display Properties
    public string FullName => $"{FirstName} {LastName}";
    public string StatusText => IsActive ? "Aktif" : "Pasif";
}
```

### CompanyViewModel

```csharp
public class CompanyViewModel
{
    public int CompanyID { get; set; }
    public string CompanyName { get; set; }
    public string CompanyCode { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public int DepartmentCount { get; set; }
    public int LayoutCount { get; set; }
    public string PrimaryColor { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

### DepartmentViewModel

```csharp
public class DepartmentViewModel
{
    public int DepartmentID { get; set; }
    public int CompanyID { get; set; }
    public string DepartmentName { get; set; }
    public string DepartmentCode { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public string CompanyName { get; set; }
    public int PageCount { get; set; }
    public int ContentCount { get; set; }
    public int ScheduleCount { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

### LayoutViewModel

```csharp
public class LayoutViewModel
{
    public int LayoutID { get; set; }
    public int CompanyID { get; set; }
    public string LayoutName { get; set; }
    public string LayoutCode { get; set; }
    public int GridColumnsX { get; set; }
    public int GridRowsY { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public int TotalSections => GridColumnsX * GridRowsY;
}
```

### DynamicLayoutViewModel (YENİ)

```csharp
public class DynamicLayoutViewModel
{
    public int LayoutID { get; set; }
    public int CompanyID { get; set; }
    public string LayoutName { get; set; }

    // Dynamic Grid Configuration
    public int GridColumnsX { get; set; }  // 1-12
    public int GridRowsY { get; set; }     // 1-12

    // Sections with coordinates
    public List<GridSectionDTO> Sections { get; set; }

    public class GridSectionDTO
    {
        public int SectionID { get; set; }
        public string Position { get; set; }
        public int Column { get; set; }
        public int Row { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
    }
}
```

### PageViewModel

```csharp
public class PageViewModel
{
    public int PageID { get; set; }
    public int DepartmentID { get; set; }
    public string PageName { get; set; }
    public string PageTitle { get; set; }
    public int LayoutID { get; set; }
    public string LayoutName { get; set; }
    public string DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public int ContentCount { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

### ContentViewModel

```csharp
public class ContentViewModel
{
    public int ContentID { get; set; }
    public int PageID { get; set; }
    public string ContentType { get; set; }
    public string ContentTitle { get; set; }
    public string ContentData { get; set; }
    public string MediaPath { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
```

### ScheduleViewModel

```csharp
public class ScheduleViewModel
{
    public int ScheduleID { get; set; }
    public int DepartmentID { get; set; }
    public string ScheduleName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
    public string RecurrencePattern { get; set; }
    public bool IsActive { get; set; }
    public int PageCount { get; set; }
}
```

### UserCompanyRoleViewModel

```csharp
public class UserCompanyRoleViewModel
{
    public int UserCompanyRoleID { get; set; }
    public int UserID { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public int CompanyID { get; set; }
    public string CompanyName { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public bool IsOffice365User { get; set; }
    public DateTime AssignedDate { get; set; }
}
```

---

## DTOs (Data Transfer Objects)

Hizmetler ve API'ler arası veri transferi için kullanılır.

### CreateUserDTO

```csharp
public class CreateUserDTO
{
    [Required]
    public string UserName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [MinLength(6)]
    public string Password { get; set; }  // Optional for Office 365 users

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsOffice365User { get; set; }
    public string AzureADObjectId { get; set; }
}
```

### UpdateCompanyDTO

```csharp
public class UpdateCompanyDTO
{
    public string CompanyName { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
}
```

### CreatePageDTO

```csharp
public class CreatePageDTO
{
    [Required]
    public int DepartmentID { get; set; }

    [Required]
    public string PageName { get; set; }

    [Required]
    public string PageTitle { get; set; }

    [Required]
    public int LayoutID { get; set; }
}
```

### CompanyConfigurationDTO

```csharp
public class CompanyConfigurationDTO
{
    public int DefaultGridColumnsX { get; set; }
    public int DefaultGridRowsY { get; set; }
    public string DefaultSectionPadding { get; set; }
    public int ScreenRefreshInterval { get; set; }
    public bool EnableAutoRotation { get; set; }
    public string CustomCSS { get; set; }
    public bool EnableAnalytics { get; set; }
    public bool EnableAdvancedScheduling { get; set; }
    public bool EnableMediaUpload { get; set; }
    public int MaxMediaSizeGB { get; set; }
}
```

### CreateContentDTO

```csharp
public class CreateContentDTO
{
    [Required]
    public int DepartmentID { get; set; }

    [Required]
    public string ContentType { get; set; }  // Text, Image, Video, HTML

    public string ContentTitle { get; set; }

    [Required]
    public string ContentData { get; set; }

    public string MediaPath { get; set; }
    public int? DurationSeconds { get; set; }
}
```

---

## Model Validations

EF Core ve Data Annotations kullanarak validasyon:

```csharp
public class Company
{
    [Key]
    public int CompanyID { get; set; }

    [Required, MaxLength(255)]
    public string CompanyName { get; set; }

    [Required, MaxLength(50)]
    public string CompanyCode { get; set; }

    [MaxLength(500)]
    public string Description { get; set; }

    [Range(0, 1)]
    public bool IsActive { get; set; }
}
```

---

## Referanslar
- [ASP.NET Core Models](https://docs.microsoft.com/aspnet/core/mvc/models/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Data Annotations](https://docs.microsoft.com/dotnet/api/system.componentmodel.dataannotations)
