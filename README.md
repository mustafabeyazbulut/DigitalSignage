# Digital Signage Platform v2.2 Professional Edition

Enterprise-grade multi-tenant digital signage management system with advanced role-based authorization built with ASP.NET Core 9.

## 🌟 Features

- **Multi-Tenant Architecture**: Company → Department → Page hierarchy
- **Advanced Authorization System**:
  - Multi-level role management (SystemAdmin → CompanyAdmin → DepartmentManager → Viewer)
  - Company-level and Department-level permissions
  - Hierarchical access control with automatic inheritance
  - Cache-optimized permission checks
- **Office 365 Integration**: Azure AD authentication with SSO
- **Dynamic Grid Layouts**: 1-12x1-12 customizable grid system
- **Multi-Language Support**: English, Turkish, German
- **CRUD Operations**: Complete management for all entities
- **Repository Pattern**: Unit of Work implementation
- **JSON-based Localization**: Easy translation management

## 🚀 Quick Start

### Prerequisites

- .NET 9 SDK
- SQL Server 2022+
- Visual Studio 2022 or VS Code

### Installation

```bash
# Clone repository
git clone https://github.com/your-org/DigitalSignage.git
cd DigitalSignage

# Restore dependencies
dotnet restore

# Update database
dotnet ef database update

# Run application
dotnet run
```

Access at: `http://localhost:5259`

## 📚 Documentation

See [.SKILLS](./.SKILLS/) folder for detailed documentation:

- [Architecture](./.SKILLS/SKILLS/01_ARCHITECTURE.md)
- [Database Schema](./.SKILLS/SKILLS/02_DATABASE_SCHEMA.md)
- [Multi-Tenant](./.SKILLS/SKILLS/08_MULTI_TENANT.md)
- [Localization](./.SKILLS/SKILLS/11_LOCALIZATION.md)

## 🛠️ Tech Stack

- **Backend**: ASP.NET Core 9 MVC
- **ORM**: Entity Framework Core 9
- **Database**: SQL Server 2022+
- **Authentication**: Cookie + OpenID Connect (Office 365)
- **Mapping**: AutoMapper
- **Localization**: JSON-based (wwwroot/lang/)

## 📦 Project Structure

```
DigitalSignage/
├── Controllers/         # MVC Controllers
├── Views/              # Razor Views
├── Models/             # Entity Models
├── ViewModels/         # View Models
├── DTOs/               # Data Transfer Objects
├── Services/           # Business Logic
├── Data/
│   ├── Repositories/   # Data Access Layer
│   └── Migrations/     # EF Migrations
├── Mappings/           # AutoMapper Profiles
├── wwwroot/
│   └── lang/          # Localization files (en, tr, de)
└── .SKILLS/           # Documentation
```

## 🌐 Multi-Language Support

The application supports 3 languages:
- 🇬🇧 English (en)
- 🇹🇷 Türkçe (tr)
- 🇩🇪 Deutsch (de)

Translation files: `wwwroot/lang/{locale}.json`

## 🔐 Security

- Role-based authorization (SystemAdmin, CompanyAdmin, DepartmentManager)
- Multi-tenant data isolation
- Input validation with DTOs
- SQL injection prevention

## 📝 Recent Updates

### v2.2.0 (2026-02-12)
- ✅ **Multi-Level Authorization System** implemented
- ✅ UserDepartmentRole model + repository + service layer
- ✅ Role management UI with AJAX-based department loading
- ✅ AuthorizationService with hierarchical permission checks
- ✅ 51 new translations for role management (EN, TR, DE)
- ✅ Comprehensive authorization documentation (12_AUTHORIZATION.md)
- ✅ Cache-optimized permission checks (10-15 min TTL)

### v2.1.1 (2025-02-12)
- ✅ User module fully localized (EN, TR, DE)
- ✅ UpdateUserDTO added for proper update operations
- ✅ Error.cshtml & AccessDenied.cshtml localized
- ✅ 93 new translations added
- ✅ All views converted to multi-language support

## 🤝 Contributing

See [CONTRIBUTING.md](./.SKILLS/CONTRIBUTING.md) for development guidelines.

## 📄 License

Proprietary - All rights reserved

---

**Version:** 2.2.0
**Last Updated:** February 12, 2026
