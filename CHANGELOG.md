# Changelog

All notable changes to the Digital Signage Platform will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [2.2.0] - 2026-02-12

### Added
- **Multi-Level Authorization System**
  - Added `UserDepartmentRole` model for department-level permissions
  - Added `IAuthorizationService` and `AuthorizationService` implementation
  - Added `IUserDepartmentRoleRepository` and implementation
  - Authorization levels: SystemAdmin → CompanyAdmin → DepartmentManager → Viewer

- **Database Schema**
  - New table: `UserDepartmentRoles` with unique composite index (UserID, DepartmentID)
  - Migration: `AddUserDepartmentRole` applied successfully
  - Navigation properties added to User and Department models

- **Role Management UI**
  - Added `ManageRoles.cshtml` view for role assignment
  - Added "Manage Roles" button to User/Index.cshtml
  - AJAX-based department loading when company is selected
  - Company and Department role assignment/removal forms

- **UserController Extensions**
  - Added `ManageRoles(int id)` action - role management page
  - Added `AssignCompanyRole` and `RemoveCompanyRole` actions
  - Added `AssignDepartmentRole` and `RemoveDepartmentRole` actions
  - Added `GetCompanyDepartments` AJAX endpoint
  - Injected `IAuthorizationService`, `ICompanyService`, `IDepartmentService`

- **DTOs & ViewModels**
  - Added `AssignCompanyRoleDTO` for company role assignment
  - Added `AssignDepartmentRoleDTO` for department role assignment
  - Added `UserDepartmentRoleDTO` for data transfer
  - Added `UserRoleManagementViewModel` for role management page
  - Added `UserDepartmentRoleViewModel` for display

- **AutoMapper Configuration**
  - Added UserDepartmentRole ↔ UserDepartmentRoleDTO mappings
  - Added UserDepartmentRole → UserDepartmentRoleViewModel mapping

- **Localization**
  - Added 17 new role management translation keys (EN, TR, DE)
  - Total: 51 new translations (17 × 3 languages)
  - Keys: role.title, role.companyRoles, role.assignCompanyRole, etc.

- **Documentation**
  - Created `.SKILLS/SKILLS/12_AUTHORIZATION.md` comprehensive guide
  - Documented role hierarchy, permission model, database schema
  - Added code examples, controller implementation patterns
  - Included migration examples, testing scenarios, best practices

### Changed
- **UnitOfWork Pattern**
  - Added `UserDepartmentRoles` repository property to IUnitOfWork and UnitOfWork
  - Lazy-initialized repository pattern maintained

- **AppDbContext**
  - Added `DbSet<UserDepartmentRole>`
  - Configured UserDepartmentRole entity with composite unique index
  - Added unique index to UserCompanyRole (UserID, CompanyID)

- **Program.cs**
  - Registered `IAuthorizationService` in DI container (Scoped lifetime)

### Technical Details
- **Files Created:** 14 new files
- **Files Modified:** 10 files
- **Total Lines Added:** ~1,500
- **Database Tables:** +1 (UserDepartmentRoles)
- **Repository Layer:** +2 files (Interface + Implementation)
- **Service Layer:** +2 files (Interface + Implementation)
- **DTOs:** +3 files
- **ViewModels:** +2 files
- **Views:** +1 file (ManageRoles.cshtml)
- **Build Status:** ✅ 0 errors, 0 warnings

### Authorization Logic
**Company Admin Behavior:**
- Company Admin automatically has access to ALL departments in that company
- No need to assign individual department roles
- Can manage all pages, content, schedules in that company

**Department Manager Behavior:**
- Can be assigned to one or multiple specific departments
- Only has access to assigned departments
- Cannot create new departments (CompanyAdmin only)

**SystemAdmin Behavior:**
- Full access to everything (all companies, all departments)
- User.IsSystemAdmin flag determines this level

### Security
- Cache-based permission checks (10-15 minute TTL)
- Hierarchical authorization (SystemAdmin > CompanyAdmin > DepartmentManager)
- Fail-safe approach (deny access when in doubt)
- Audit logging for all role assignments/removals

---

## [2.1.1] - 2025-02-12

### Added
- **User Module Multi-Language Support**
  - Added full localization for all User views (Index, Create, Edit, Details, Delete)
  - Added 93 new translation keys across 3 languages (EN, TR, DE)
  - Added `UpdateUserDTO` for proper user update operations
  - Added XML documentation to `UpdateUserDTO`

- **Error Pages Localization**
  - Localized `Error.cshtml` with error section translations
  - Localized `AccessDenied.cshtml` with auth section translations
  - Added `auth.contactAdmin` and `auth.goToHome` keys

### Changed
- **UserController**
  - Replaced all hardcoded error messages with localization keys
  - Updated Edit action to use `UpdateUserDTO` instead of `UserViewModel`
  - Removed unused exception variables (ex → Exception)
  - All success and error messages now support 3 languages

- **UserViewModel**
  - Removed hardcoded `StatusText` property
  - Views now use `T("common.active")` and `T("common.inactive")`

- **AutoMapper Configuration**
  - Added `UpdateUserDTO → User` mapping with proper ignores
  - Added `User → UpdateUserDTO` reverse mapping

### Fixed
- Build warnings eliminated (0 warnings, 0 errors)
- Edit view now correctly uses DTO pattern instead of ViewModel
- Status text properly localized in all views

### Translation Keys Added
**User Module (29 keys):**
- `user.title`, `user.subtitle`, `user.newUser`
- `user.createTitle`, `user.editTitle`, `user.detailsTitle`, `user.deleteTitle`
- `user.userName`, `user.email`, `user.password`, `user.firstName`, `user.lastName`
- `user.isOffice365User`, `user.office365`, `user.local`
- `user.passwordIncorrect`, `user.passwordChanged`
- `user.errorLoading`, `user.errorCreating`, `user.errorUpdating`, `user.errorDeleting`
- And 10 more...

**Common (2 keys):**
- `common.previous`, `common.next`

**Error (8 keys):**
- `error.title`, `error.heading`, `error.message`, etc.

**Auth (2 keys):**
- `auth.contactAdmin`, `auth.goToHome`

### Technical Details
- **Files Modified:** 13 files
- **Total Changes:** 171+ modifications
- **Lines Added:** ~500
- **Lines Removed:** ~50
- **DTO Pattern:** Fully implemented for User module

---

## [2.1.0] - 2025-02-10

### Added
- Initial multi-language support structure
- Company, Department, Page, Layout modules with localization
- JSON-based translation system (EN, TR, DE)

### Changed
- Migrated to ASP.NET Core 9
- Updated to EF Core 9
- Modern repository pattern implementation

---

## [2.0.0] - 2025-02-09

### Added
- Multi-tenant architecture
- Office 365 SSO integration
- Dynamic grid layout system (1-12x1-12)
- Repository Pattern with Unit of Work
- CRUD operations for all entities

### Infrastructure
- ASP.NET Core 9 MVC
- Entity Framework Core 9
- SQL Server 2022+
- AutoMapper integration

---

**Note:** This changelog started with version 2.1.1. Previous versions are documented retroactively.
