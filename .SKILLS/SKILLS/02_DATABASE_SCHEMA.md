# Veritabanı Şeması

## Genel Bakış

SQL Server 2022+ tabanlı relational veritabanı. Tüm tablolar, constraints ve ilişkiler detaylı olarak açıklanmıştır.

## ER Diyagramı (Entity Relationship Diagram)

```
┌─────────────────┐
│   Companies     │
│ (Şirketler)     │
└────────┬────────┘
         │
    ┌────▼──────────────┐
    │ Departments       │
    │(Departmanlar)     │
    └────────┬──────────┘
             │
    ┌────────┼────────────────────┬──────────────┐
    │        │                    │              │
┌───▼──┐  ┌──▼───┐          ┌─────▼────┐    ┌──▼────┐
│Pages │  │ User │          │Schedules │    │Content│
│      │  │      │          │          │    │(Reusable)
└───┬──┘  └──┬───┘          └──────────┘    └──┬────┘
    │        │                                  │
    │   ┌────▼──────────────┐           ┌──────▼────────┐
    │   │UserCompanyRoles  │           │PageContent(N:N)
    │   │(Kullanıcı-Şirket)│           │(Page↔Content) │
    │   └───────────────────┘           └───────────────┘
    │
┌───▼──────────────────┐
│ Layouts              │
│ (Dinamik Grid X-Y)   │
└───┬──────────────────┘
    │
┌───▼────────────────┐
│ LayoutSections     │
│ (Bölümler)         │
└────────────────────┘
```

---

## Ilişki Özeti

- **Company** → **Department** (1:N)
- **Department** → **Page** (1:N)
- **Department** → **User** (1:N) - via UserCompanyRole
- **Department** → **Schedule** (1:N)
- **Department** → **Content** (1:N) - Content ait olduğu departman
- **Page** ↔ **Content** (N:N) - via PageContent (bir content birden fazla page'de kullanılabilir)
- **Page** → **Layout** (N:1)
- **Layout** → **LayoutSection** (1:N)
- **User** ↔ **Company** (N:N) - via UserCompanyRole
- **Schedule** → **SchedulePage** → **Page** (1:N:N)

---

## Tablo Tanımları

### 1. COMPANIES (Şirketler)

**Amaç**: Sistem içinde farklı şirketleri temsil eder.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| CompanyID | int | NO | IDENTITY(1,1) | Primary Key |
| CompanyName | nvarchar(255) | NO | - | Şirket adı |
| CompanyCode | nvarchar(50) | NO | - | Şirket kodu (unique) |
| Description | nvarchar(500) | YES | - | Şirket açıklaması |
| IsActive | bit | NO | 1 | Şirket aktif mi? |
| CreatedDate | datetime | NO | GETDATE() | Oluşturma tarihi |
| CreatedBy | nvarchar(255) | NO | - | Oluşturan kullanıcı |
| ModifiedDate | datetime | YES | NULL | Değiştirilme tarihi |
| ModifiedBy | nvarchar(255) | YES | - | Değiştiren kullanıcı |

**Constraints**:
- PRIMARY KEY: CompanyID
- UNIQUE: CompanyCode

---

### 2. USERS (Kullanıcılar)

**Amaç**: Sistem kullanıcılarını temsil eder.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| UserID | int | NO | IDENTITY(1,1) | Primary Key |
| UserName | nvarchar(255) | NO | - | Kullanıcı adı (unique) |
| Email | nvarchar(255) | NO | - | Email adresi (unique) |
| PasswordHash | nvarchar(max) | NO | - | Şifrelenmiş parola |
| FirstName | nvarchar(255) | YES | - | Ad |
| LastName | nvarchar(255) | YES | - | Soyadı |
| IsActive | bit | NO | 1 | Kullanıcı aktif mi? |
| IsSystemAdmin | bit | NO | 0 | Sistem yöneticisi mi? |
| CreatedDate | datetime | NO | GETDATE() | Oluşturma tarihi |
| LastLoginDate | datetime | YES | NULL | Son giriş tarihi |
| ModifiedDate | datetime | YES | NULL | Değiştirilme tarihi |

**Constraints**:
- PRIMARY KEY: UserID
- UNIQUE: UserName
- UNIQUE: Email

---

### 3. USER_COMPANY_ROLES (Kullanıcı-Şirket Rolleri)

**Amaç**: Kullanıcıların şirketlerdeki rollerini tanımlar. Bir kullanıcı birden fazla şirkette farklı roller sahibi olabilir.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| UserCompanyRoleID | int | NO | IDENTITY(1,1) | Primary Key |
| UserID | int | NO | - | Foreign Key (Users) |
| CompanyID | int | NO | - | Foreign Key (Companies) |
| Role | nvarchar(50) | NO | - | Rol (SystemAdmin, CompanyAdmin, Manager, Editor, Viewer) |
| IsActive | bit | NO | 1 | Rol aktif mi? |
| AssignedDate | datetime | NO | GETDATE() | Atama tarihi |
| AssignedBy | nvarchar(255) | NO | - | Atayan kullanıcı |

**Constraints**:
- PRIMARY KEY: UserCompanyRoleID
- FOREIGN KEY: UserID → Users(UserID) [CASCADE DELETE]
- FOREIGN KEY: CompanyID → Companies(CompanyID) [CASCADE DELETE]
- UNIQUE: UserID + CompanyID + Role

---

### 4. DEPARTMENTS (Departmanlar)

**Amaç**: Şirket içinde departmanları temsil eder. Sayfalar, içerikler ve zamanlamalar departman düzeyinde yönetilir.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| DepartmentID | int | NO | IDENTITY(1,1) | Primary Key |
| CompanyID | int | NO | - | Foreign Key (Companies) |
| DepartmentName | nvarchar(255) | NO | - | Departman adı |
| DepartmentCode | nvarchar(50) | NO | - | Departman kodu |
| Description | nvarchar(500) | YES | - | Açıklama |
| IsActive | bit | NO | 1 | Departman aktif mi? |
| CreatedDate | datetime | NO | GETDATE() | Oluşturma tarihi |
| CreatedBy | nvarchar(255) | NO | - | Oluşturan |

**Constraints**:
- PRIMARY KEY: DepartmentID
- FOREIGN KEY: CompanyID → Companies(CompanyID) [CASCADE DELETE]
- UNIQUE: CompanyID + DepartmentCode

---

### 5. PAGES (Sayfalar)

**Amaç**: Dijital işaretleme ekranlarında gösterilecek sayfaları temsil eder.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| PageID | int | NO | IDENTITY(1,1) | Primary Key |
| DepartmentID | int | NO | - | Foreign Key (Departments) |
| PageName | nvarchar(255) | NO | - | Sayfa adı |
| PageTitle | nvarchar(255) | NO | - | Sayfa başlığı |
| PageCode | nvarchar(50) | NO | - | Sayfa kodu |
| LayoutID | int | NO | - | Foreign Key (Layouts) |
| Description | nvarchar(500) | YES | - | Açıklama |
| IsActive | bit | NO | 1 | Sayfa aktif mi? |
| CreatedDate | datetime | NO | GETDATE() | Oluşturma tarihi |

**Constraints**:
- PRIMARY KEY: PageID
- FOREIGN KEY: DepartmentID → Departments(DepartmentID) [CASCADE DELETE]
- FOREIGN KEY: LayoutID → Layouts(LayoutID)

---

### 6. LAYOUTS (Sayfa Tasarımları)

**Amaç**: Sayfaların dinamik grid yapısını tanımlar (X-Y grid).

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| LayoutID | int | NO | IDENTITY(1,1) | Primary Key |
| CompanyID | int | NO | - | Foreign Key (Companies) |
| LayoutName | nvarchar(255) | NO | - | Düzen adı |
| LayoutCode | nvarchar(50) | NO | - | Düzen kodu |
| GridColumnsX | int | NO | 1 | X ekseninde (yatay) kaç bölüm |
| GridRowsY | int | NO | 1 | Y ekseninde (dikey) kaç bölüm |
| Description | nvarchar(500) | YES | - | Açıklama |
| IsActive | bit | NO | 1 | Düzen aktif mi? |
| CreatedDate | datetime | NO | GETDATE() | Oluşturma tarihi |

**Constraints**:
- PRIMARY KEY: LayoutID
- FOREIGN KEY: CompanyID → Companies(CompanyID) [CASCADE DELETE]
- UNIQUE: CompanyID + LayoutCode

---

### 7. LAYOUT_SECTIONS (Düzen Bölümleri)

**Amaç**: Her düzende kaç bölüm olduğunu ve özelliklerini tanımlar.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| LayoutSectionID | int | NO | IDENTITY(1,1) | Primary Key |
| LayoutID | int | NO | - | Foreign Key (Layouts) |
| SectionPosition | nvarchar(50) | NO | - | Konum (A1, A2, B1 vb.) |
| ColumnIndex | int | NO | - | X koordinatı (0-indexed) |
| RowIndex | int | NO | - | Y koordinatı (0-indexed) |
| Width | nvarchar(50) | YES | "100%" | Genişlik (CSS) |
| Height | nvarchar(50) | YES | "100%" | Yükseklik (CSS) |
| IsActive | bit | NO | 1 | Bölüm aktif mi? |

**Constraints**:
- PRIMARY KEY: LayoutSectionID
- FOREIGN KEY: LayoutID → Layouts(LayoutID) [CASCADE DELETE]
- UNIQUE: LayoutID + SectionPosition

---

### 8. CONTENTS (İçerikler - Reusable)

**Amaç**: Sayfalar üzerinde gösterilecek içerikleri temsil eder. Bir content birden fazla sayfada kullanılabilir.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| ContentID | int | NO | IDENTITY(1,1) | Primary Key |
| DepartmentID | int | NO | - | Foreign Key (Departments) |
| ContentType | nvarchar(50) | NO | - | Tip (Text, Image, Video, HTML) |
| ContentTitle | nvarchar(255) | YES | - | İçerik başlığı |
| ContentData | nvarchar(max) | NO | - | İçerik verisi |
| MediaPath | nvarchar(500) | YES | - | Medya dosya yolu |
| ThumbnailPath | nvarchar(500) | YES | - | Thumbnail resmi |
| DurationSeconds | int | YES | - | Video/Slide süresi (saniye) |
| IsActive | bit | NO | 1 | İçerik aktif mi? |
| CreatedDate | datetime | NO | GETDATE() | Oluşturma tarihi |
| CreatedBy | nvarchar(255) | NO | - | Oluşturan |
| ModifiedDate | datetime | YES | NULL | Değiştirilme tarihi |

**Constraints**:
- PRIMARY KEY: ContentID
- FOREIGN KEY: DepartmentID → Departments(DepartmentID) [CASCADE DELETE]

---

### 8.1 PAGE_CONTENT (Sayfa-İçerik İlişkisi - N:N)

**Amaç**: Sayfalar ile içerikler arasındaki N:N ilişkisini yönetir. Bir content birden fazla sayfada kullanılabilir.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| PageContentID | int | NO | IDENTITY(1,1) | Primary Key |
| PageID | int | NO | - | Foreign Key (Pages) |
| ContentID | int | NO | - | Foreign Key (Contents) |
| DisplayOrder | int | NO | 0 | Sayfadaki gösterim sırası |
| DisplaySection | nvarchar(50) | YES | - | Hangi layout section'da gösterilecek |
| IsActive | bit | NO | 1 | Aktif mi? |
| AddedDate | datetime | NO | GETDATE() | Eklenme tarihi |

**Constraints**:
- PRIMARY KEY: PageContentID
- FOREIGN KEY: PageID → Pages(PageID) [CASCADE DELETE]
- FOREIGN KEY: ContentID → Contents(ContentID) [CASCADE DELETE]
- UNIQUE: PageID + ContentID

---

### 9. PAGE_SECTIONS (Sayfa-Layout Bölümleri)

**Amaç**: Sayfaların layout bölümleriyle olan yapısını tanımlar. Her page bir layout'un bölümlerini kullanır.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| PageSectionID | int | NO | IDENTITY(1,1) | Primary Key |
| PageID | int | NO | - | Foreign Key (Pages) |
| LayoutSectionID | int | NO | - | Foreign Key (LayoutSections) |
| IsActive | bit | NO | 1 | Bölüm aktif mi? |

**Constraints**:
- PRIMARY KEY: PageSectionID
- FOREIGN KEY: PageID → Pages(PageID) [CASCADE DELETE]
- FOREIGN KEY: LayoutSectionID → LayoutSections(LayoutSectionID)
- UNIQUE: PageID + LayoutSectionID

---

### 10. SCHEDULES (Zaman Çizelgeleri)

**Amaç**: Sayfaların ne zaman gösterileneceğini tanımlar.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| ScheduleID | int | NO | IDENTITY(1,1) | Primary Key |
| DepartmentID | int | NO | - | Foreign Key (Departments) |
| ScheduleName | nvarchar(255) | NO | - | Çizelge adı |
| StartDate | datetime | NO | - | Başlangıç tarihi |
| EndDate | datetime | NO | - | Bitiş tarihi |
| StartTime | time | NO | - | Başlangıç saati |
| EndTime | time | NO | - | Bitiş saati |
| IsRecurring | bit | NO | 0 | Yinelenen mi? |
| RecurrencePattern | nvarchar(50) | YES | - | Yineleme şekli (Daily, Weekly, Monthly) |
| IsActive | bit | NO | 1 | Çizelge aktif mi? |
| CreatedDate | datetime | NO | GETDATE() | Oluşturma tarihi |

**Constraints**:
- PRIMARY KEY: ScheduleID
- FOREIGN KEY: DepartmentID → Departments(DepartmentID) [CASCADE DELETE]

---

### 11. SCHEDULE_PAGES (Çizelge Sayfaları)

**Amaç**: Hangi sayfaların hangi çizelgelerde gösterileneceğini tanımlar.

| Kolon | Tip | Nullable | Default | Açıklama |
|-------|-----|----------|---------|----------|
| SchedulePageID | int | NO | IDENTITY(1,1) | Primary Key |
| ScheduleID | int | NO | - | Foreign Key (Schedules) |
| PageID | int | NO | - | Foreign Key (Pages) |
| DisplayDuration | int | NO | 30 | Gösterim süresi (saniye) |
| DisplayOrder | int | NO | 0 | Gösterim sırası |
| IsActive | bit | NO | 1 | Aktif mi? |

**Constraints**:
- PRIMARY KEY: SchedulePageID
- FOREIGN KEY: ScheduleID → Schedules(ScheduleID) [CASCADE DELETE]
- FOREIGN KEY: PageID → Pages(PageID)
- UNIQUE: ScheduleID + PageID

---

## İndeks Tanımları

```sql
-- Performance Indexes
CREATE INDEX IX_Users_UserName ON Users(UserName);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Companies_CompanyCode ON Companies(CompanyCode);
CREATE INDEX IX_Departments_CompanyID ON Departments(CompanyID);
CREATE INDEX IX_Pages_DepartmentID ON Pages(DepartmentID);
CREATE INDEX IX_Pages_LayoutID ON Pages(LayoutID);
CREATE INDEX IX_Layouts_CompanyID ON Layouts(CompanyID);

-- Content Indexes (Reusable Content)
CREATE INDEX IX_Contents_DepartmentID ON Contents(DepartmentID);
CREATE INDEX IX_Contents_ContentType ON Contents(ContentType);
CREATE INDEX IX_PageContent_PageID ON PageContent(PageID);
CREATE INDEX IX_PageContent_ContentID ON PageContent(ContentID);
CREATE INDEX IX_PageContent_DisplayOrder ON PageContent(PageID, DisplayOrder);

-- Schedule Indexes
CREATE INDEX IX_Schedules_DepartmentID ON Schedules(DepartmentID);
CREATE INDEX IX_Schedules_StartDate ON Schedules(StartDate);
CREATE INDEX IX_SchedulePages_ScheduleID ON SchedulePages(ScheduleID);
CREATE INDEX IX_SchedulePages_PageID ON SchedulePages(PageID);

-- User & Role Indexes
CREATE INDEX IX_UserCompanyRoles_UserID ON UserCompanyRoles(UserID);
CREATE INDEX IX_UserCompanyRoles_CompanyID ON UserCompanyRoles(CompanyID);
CREATE INDEX IX_PageSections_PageID ON PageSections(PageID);
CREATE INDEX IX_PageSections_LayoutSectionID ON PageSections(LayoutSectionID);
```

---

## Veri Bütünlüğü Kuralları

### Cascading Rules
- Şirket silinirse → Departman, Sayfalar silinir
- Departman silinirse → Sayfalar, Çizelgeler silinir
- Sayfa silinirse → İçerikler, Sayfa Bölümleri silinir
- Çizelge silinirse → Çizelge Sayfaları silinir

### Validasyon Kuralları
- GridColumnsX ve GridRowsY > 0
- StartDate ≤ EndDate (Schedule)
- StartTime < EndTime (Schedule)
- Email unique ve valid format
- PasswordHash minimum length

---

## Referanslar
- [SQL Server Documentation](https://docs.microsoft.com/sql/)
- [Entity Framework Core Conventions](https://docs.microsoft.com/ef/core/modeling/)
