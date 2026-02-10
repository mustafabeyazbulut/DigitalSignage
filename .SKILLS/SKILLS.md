# Digital Signage - Professional Skills & Architecture

**Versiyon**: 2.0 (Professional Edition)
**Framework**: ASP.NET Core 9 MVC
**Database**: SQL Server 2022+
**ORM**: Entity Framework Core 9
**Authentication**: Office 365 (Azure AD) + Identity
**Architecture**: Enterprise N-Tier, Fully Dynamic

---

## 📋 İçindekiler

1. [Sistem Mimarisi](./SKILLS/01_ARCHITECTURE.md)
2. [Veritabanı Şeması](./SKILLS/02_DATABASE_SCHEMA.md)
3. [Veri Modelleri & ViewModels](./SKILLS/03_DATA_MODELS.md)
4. [ORM & Data Access Layer](./SKILLS/04_ORM_DATA_ACCESS.md)
5. [Business Logic & Services](./SKILLS/05_BUSINESS_LOGIC.md)
6. [MVC Controllers & Views](./SKILLS/06_MVC_LAYER.md)
7. [Office 365 Authentication](./SKILLS/07_OFFICE365_AUTH.md)
8. [Multi-Tenant Company System](./SKILLS/08_MULTI_TENANT.md)
9. [Dinamik Sayfa Tasarımları (Grid System)](./SKILLS/09_DYNAMIC_LAYOUTS.md)
10. [Deployment & Maintenance](./SKILLS/10_DEPLOYMENT.md)

---

## 🏗️ Proje Hiyerarşisi (YENİ)

```
Digital Signage Platform
│
├── Company (Şirket) ──────────────── Multi-Tenant
│   ├── Department (Departman)
│   │   ├── Page (Sayfa)
│   │   │   ├── Layout (Dinamik Grid X-Y)
│   │   │   │   └── Section (Bölüm)
│   │   │   │       └── Content (İçerik)
│   │   │   └── Schedule (Zamanlama)
│   │   └── User (Rol ile)
│   │       ├── CompanyAdmin
│   │       ├── DepartmentManager
│   │       └── User
│   │
│   └── Configuration (Dinamik Ayarlar)
│       ├── EmailSettings
│       ├── NotificationSettings
│       ├── LayoutDefaults
│       └── ScheduleRules
│
└── Global Configuration
    ├── Office365 Settings
    ├── System Preferences
    └── Feature Flags
```

---

## ✨ Temel Özellikler

### 🔐 Güvenlik & Authentication
- ✅ Office 365 (Azure AD) Integration
- ✅ Multi-factor Authentication (MFA)
- ✅ Role-based Access Control (RBAC)
- ✅ Claims-based Authorization
- ✅ Session Management

### 🏢 Multi-Tenant
- ✅ Company-level Data Isolation
- ✅ Tenant Context Management
- ✅ Per-company Customization
- ✅ Independent Configuration

### 🎨 Dinamik Sayfa Tasarımı
- ✅ X-Y Grid System (1-12 columns/rows)
- ✅ Responsive Design
- ✅ Real-time Preview
- ✅ Custom CSS per Layout

### ⚙️ Dinamik Konfigürasyon
- ✅ Database-driven Settings
- ✅ Feature Toggles
- ✅ Per-company Customization
- ✅ Role-based Permissions

### 📅 İçerik & Zamanlama
- ✅ Content Management System
- ✅ Schedule Management
- ✅ Recurring Schedules
- ✅ Media Support (Image, Video, HTML)

---

## 🎯 Teknoloji Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | ASP.NET Core 9 MVC |
| **ORM** | Entity Framework Core 9 |
| **Database** | SQL Server 2022+ |
| **Auth** | Azure AD (Office 365) + Identity |
| **Frontend** | Bootstrap 5, JavaScript, Responsive CSS |
| **Caching** | In-Memory/Redis |
| **Logging** | Serilog, Application Insights |
| **Testing** | xUnit, Moq |
| **Deployment** | Docker, Azure App Service, IIS |

---

## 🚀 Başlangıç

### Development Kurulumu
Bkz. [Deployment Guide](./SKILLS/10_DEPLOYMENT.md#development-environment-setup)

### Database Setup
Bkz. [Database Schema](./SKILLS/02_DATABASE_SCHEMA.md)

### First Run
```bash
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

---

## 👥 Kullanıcı Rolleri

| Role | Yetkiler |
|------|----------|
| **System Admin** | Tüm platform yönetimi |
| **Company Admin** | Şirket yönetimi, tüm departmanlar |
| **Department Manager** | Departman yönetimi, sayfa/içerik |
| **Content Editor** | İçerik oluştur/düzenle |
| **Viewer** | Sadece okuma |

---

## 🔒 Güvenlik Önlemleri

- ✅ HTTPS/TLS enforced
- ✅ SQL Injection prevention
- ✅ XSS protection
- ✅ CSRF token validation
- ✅ Password hashing (bcrypt)
- ✅ Rate limiting
- ✅ Audit logging
- ✅ Data encryption at rest
- ✅ Office 365 MFA Integration

---

## 📞 Destek & Bakım

### Monitoring
- Application Insights
- Performance metrics
- Error tracking
- User analytics

### Logging
- Structured logging (Serilog)
- Audit trails
- Security events
- Performance profiling

### Backup & Recovery
- Automated backups
- Point-in-time recovery
- Disaster recovery plan

---

## 🔄 Sürüm Geçmişi

| Versiyon | Tarih | Değişiklikler |
|----------|-------|---------------|
| 2.0 | Feb 2025 | Office 365 Auth, Dinamik Config, Sistem Kaldırıldı |
| 1.0 | 2024 | Initial Release |

---

## 📝 Son Güncelleme

**Tarih**: 9 Şubat 2025
**Güncelleyen**: Development Team
**Versiyon**: 2.0 Professional Edition

---

**Digital Signage - Enterprise Solution for Modern Displays**
