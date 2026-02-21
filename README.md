# Digital Signage Platform v2.3 Professional Edition

Enterprise-grade multi-tenant digital signage management system with advanced role-based authorization built with ASP.NET Core 9.

## Features

- **Multi-Tenant Architecture**: Company > Department > Page hierarchy
- **Advanced Authorization System**:
  - Multi-level role management (SystemAdmin > CompanyAdmin > DepartmentManager > Editor > Viewer)
  - Company-level and Department-level permissions
  - Hierarchical access control with automatic inheritance
  - Cache-optimized permission checks
- **Office 365 Integration**: Azure AD authentication with SSO
- **Dynamic Grid Layouts**: 1-12x1-12 customizable grid system with Design page
- **Auto-Generated Page Codes**: Sequential PG-00001 pattern
- **Multi-Language Support**: English, Turkish, German
- **Repository Pattern**: Unit of Work implementation
- **JSON-based Localization**: Easy translation management

## Quick Start

### Prerequisites

- .NET 9 SDK
- SQL Server 2022+
- Visual Studio 2022 or VS Code

### Installation

```bash
# Clone repository
git clone https://github.com/mustafabeyazbulut/DigitalSignage.git
cd DigitalSignage

# Restore dependencies
dotnet restore
```

### Database Configuration

**appsettings.json contains an empty connection string (committed to Git).**

Configure your local settings:

#### Option A: appsettings.Development.json (Recommended)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=DigitalSignageDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=false;Connection Timeout=30"
  }
}
```
**NOTE:** This file is in `.gitignore` - it will NOT be pushed to Git.

#### Option B: User Secrets (Most Secure)
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=DigitalSignageDb;User Id=sa;Password=YOUR_PASSWORD;..."
```

### Database Update & Run

```bash
# Update database
dotnet ef database update

# Run application
dotnet run
```

**On first run:**
- Admin user is automatically created
- **Random password** is printed to console (save it!)
- Sample company and departments are created

Access at: `http://localhost:5259`

## Tech Stack

- **Backend**: ASP.NET Core 9 MVC
- **ORM**: Entity Framework Core 9
- **Database**: SQL Server 2022+
- **Authentication**: Cookie + OpenID Connect (Office 365)
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **Localization**: JSON-based (wwwroot/lang/)

## Project Structure

```
DigitalSignage/
├── Controllers/         # MVC Controllers (BaseController inherited)
├── Views/               # Razor Views
├── Models/              # Entity Models
├── ViewModels/          # View Models
├── DTOs/                # Data Transfer Objects
├── Validators/          # FluentValidation validators
├── Services/            # Business Logic
├── Data/
│   ├── Repositories/    # Data Access Layer
│   └── AppDbContext.cs  # EF Core DbContext
├── Migrations/          # EF Migrations
├── Mappings/            # AutoMapper Profiles
├── Middleware/           # TenantResolver etc.
├── ViewComponents/      # CompanySelector etc.
├── wwwroot/
│   └── lang/            # Localization files (en, tr, de)
└── Helpers/             # PasswordHelper etc.
```

## Multi-Language Support

The application supports 3 languages:
- English (en)
- Turkish (tr)
- German (de)

Translation files: `wwwroot/lang/{locale}.json`

## Security

- Role-based authorization (SystemAdmin, CompanyAdmin, DepartmentManager, Editor, Viewer)
- Multi-tenant data isolation at service/controller level
- Input validation with FluentValidation DTOs
- CSRF protection with ValidateAntiForgeryToken
- Security headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection, CSP)
- Password hashing with BCrypt
- Last SystemAdmin protection (cannot delete/deactivate)

## Recent Updates

### v2.3.0 (2026-02-22)
- Page creation flow redesigned: auto-generated PageCode (PG-00001), layout selection moved to Design page
- LayoutID made nullable - pages can exist without a layout initially
- Unique indexes added for PageCode and PageName
- Layout selector partial view with grid previews
- UserName field removed from entire codebase - Email is the unique identifier
- CompanySelectorViewComponent fixed to filter by user authorization
- Company selector and language switcher positions swapped in navbar
- Obsolete SKILLS documentation files cleaned up

### v2.2.1 (2026-02-12)
- Email Notification Settings - Users can enable/disable email notifications
- Profile and Settings pages fully localized (EN, TR, DE)

### v2.2.0 (2026-02-12)
- Multi-Level Authorization System implemented
- UserDepartmentRole model + repository + service layer
- Role management UI with AJAX-based department loading
- AuthorizationService with hierarchical permission checks
- Cache-optimized permission checks (10-15 min TTL)

## License

Proprietary - All rights reserved

---

**Version:** 2.3.0
**Last Updated:** February 22, 2026
