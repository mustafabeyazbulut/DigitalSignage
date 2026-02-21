# Yetkilendirme & Veri Filtreleme — Kapsamlı İmplementasyon Planı

## Mevcut Durum Analizi

### Veri Hiyerarşisi
```
Company (root)
├── Layout (CompanyID)           ← Şirket seviyesinde paylaşılan şablon
│   └── LayoutSection            ← Grid hücreleri (A1, B1, ...)
├── Department (CompanyID)
│   ├── Page (DepartmentID + LayoutID)  ← Departman ama şirket Layout'unu kullanır
│   │   ├── PageContent → Content
│   │   ├── PageSection → LayoutSection
│   │   └── SchedulePage → Schedule
│   ├── Content (DepartmentID)
│   └── Schedule (DepartmentID)
└── UserCompanyRole (UserID + CompanyID)

User
├── IsSystemAdmin (bool)
├── UserCompanyRole → "CompanyAdmin" | "Viewer"
└── UserDepartmentRole → "DepartmentManager" | "Editor" | "Viewer"
```

---

## Tespit Edilen Sorunlar (Öncelik Sırasıyla)

### KRİTİK — Veri Sızıntısı

| # | Sorun | Dosya | Satır | Etki |
|---|-------|-------|-------|------|
| K1 | **PageController: TÜM Layout'lar filtresiz gösterilyor** | PageController.cs | Create/Edit | Kullanıcı başka şirketlerin Layout'larını görüp seçebilir |
| K2 | **PageController: TÜM Department'lar filtresiz** | PageController.cs | Create/Edit | Başka şirket departmanları dropdown'da görünür |
| K3 | **ContentController: TÜM Department'lar filtresiz** | ContentController.cs | Create/Edit | Aynı sorun |
| K4 | **ScheduleController: TÜM Department'lar filtresiz** | ScheduleController.cs | Create/Edit | Aynı sorun |
| K5 | **LayoutController: Session'dan CompanyID alınıyor, yetki kontrolü yok** | LayoutController.cs | Index | Eski session değeri ile yetkisiz şirket verisi görülebilir |

### YÜKSEK — Rol ve Yetki Boşlukları

| # | Sorun | Etki |
|---|-------|------|
| Y1 | **Editor rolü pratikte Viewer ile aynı** — `IsDepartmentManagerAsync` sadece `"DepartmentManager"` kontrol ediyor | Editor'ler hiçbir yazma işlemi yapamaz |
| Y2 | **Navigasyon menüsü herkese aynı görünüyor** — Companies, Users menüsü yetkisiz kullanıcılara da açık | 403 hataları, kötü UX |
| Y3 | **View'larda Create/Edit/Delete butonları herkese görünüyor** — Rol bazlı gizleme yok | Yetkisiz kullanıcı butona tıklayıp AccessDenied alır |
| Y4 | **CompanyAdmin kendi CompanyAdmin rolünü kaldırabilir** — Self-check yok | Kilitlenme riski |
| Y5 | **DepartmentRole atanırken CompanyRole zorunlu değil** — Tutarsız erişim | Departmana erişir ama şirkete erişemez |

### ORTA — UX ve Bağlam Sorunları

| # | Sorun | Etki |
|---|-------|------|
| O1 | **Layout departman bağlamında filtrelenmiyor** — Tüm şirket Layout'ları görünür | Kullanıcı çalıştığı departmanla ilgisiz Layout'ları görmek zorunda |
| O2 | **Dashboard tüm kullanıcılara aynı** — İstatistikler scope'lanmamış | Hızlı aksiyonlar herkese görünür |
| O3 | **Company Viewer pratikte işe yaramaz** — Departmanlara erişemez | Rol atanmış ama işlevsel değil |
| O4 | **CompanyController.Index sadece SystemAdmin** — CompanyAdmin kendi şirketini listede göremez | CompanyAdmin doğrudan URL ile Details'e gidebilir ama listeyi göremez |

---

## İmplementasyon Planı

### Faz 1: Editor Rolü Tanımlama

**Amaç:** "Editor" rolünü işlevsel hale getir. Editor = Oluşturma + Düzenleme (Silme hariç)

**Değişiklikler:**

1. **`IAuthorizationService` — yeni metod ekle:**
```csharp
// Mevcut: IsDepartmentManagerAsync → sadece DepartmentManager + CompanyAdmin + SystemAdmin
// Yeni: CanEditInDepartmentAsync → DepartmentManager VEYA Editor + CompanyAdmin + SystemAdmin
Task<bool> CanEditInDepartmentAsync(int userId, int departmentId);
```

2. **`AuthorizationService` implementasyonu:**
```csharp
public async Task<bool> CanEditInDepartmentAsync(int userId, int departmentId)
{
    if (await IsSystemAdminAsync(userId)) return true;

    var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
    if (department == null) return false;

    if (await IsCompanyAdminAsync(userId, department.CompanyID)) return true;

    var role = await GetDepartmentRoleAsync(userId, departmentId);
    return role?.Role == "DepartmentManager" || role?.Role == "Editor";
}
```

3. **Controller'larda güncelleme:**

| Controller | Action | Eski Kontrol | Yeni Kontrol |
|-----------|--------|-------------|-------------|
| ContentController | Create, Edit | `IsDepartmentManagerAsync` | `CanEditInDepartmentAsync` |
| ContentController | Delete | `IsDepartmentManagerAsync` | `IsDepartmentManagerAsync` (aynı kalır) |
| ScheduleController | Create, Edit | `IsDepartmentManagerAsync` | `CanEditInDepartmentAsync` |
| ScheduleController | Delete | `IsDepartmentManagerAsync` | `IsDepartmentManagerAsync` (aynı kalır) |
| PageController | Create, Edit | `IsDepartmentManagerAsync` / `CanModifyPageAsync` | `CanEditInDepartmentAsync` / yeni `CanEditPageAsync` |
| PageController | Delete | `CanModifyPageAsync` | `CanModifyPageAsync` (aynı kalır) |

**Yani:**
- **Editor:** Create + Edit yapabilir, Delete yapamaz
- **DepartmentManager:** Create + Edit + Delete yapabilir
- **Viewer:** Hiçbir yazma işlemi yapamaz

4. **`IAuthorizationService` — Page seviyesi yeni metod:**
```csharp
Task<bool> CanEditPageAsync(int userId, int pageId);
```

---

### Faz 2: Veri Filtreleme Düzeltmeleri (KRİTİK)

**Amaç:** Her dropdown ve liste sadece kullanıcının erişebildiği veriyi göstersin.

#### 2.1 PageController — Layout ve Department Filtreleme

**Sorun:** `ViewBag.Layouts = await _layoutService.GetAllAsync()` — TÜM şirketlerin layout'ları geliyor.

**Çözüm:**
```csharp
// Create GET — departmentId varsa onun şirketinin layout'larını getir
var department = await _departmentService.GetByIdAsync(departmentId);
ViewBag.Layouts = await _layoutService.GetByCompanyIdAsync(department.CompanyID);

// Create GET — departmentId yoksa kullanıcının erişebildiği şirketlerin layout'larını getir
var userCompanies = await _authService.GetUserCompaniesAsync(userId);
var layouts = new List<Layout>();
foreach (var company in userCompanies)
    layouts.AddRange(await _layoutService.GetByCompanyIdAsync(company.CompanyID));
ViewBag.Layouts = layouts;
```

**Departments da aynı şekilde:**
```csharp
// Eskisi: ViewBag.Departments = await _departmentService.GetAllAsync();
// Yenisi:
var userCompanies = await _authService.GetUserCompaniesAsync(userId);
var departments = new List<Department>();
foreach (var company in userCompanies)
    departments.AddRange(await _authService.GetUserDepartmentsAsync(userId, company.CompanyID));
ViewBag.Departments = departments;
```

#### 2.2 ContentController — Department Filtreleme

**Aynı pattern:** `GetAllAsync()` yerine kullanıcının erişebildiği departmanlar.

#### 2.3 ScheduleController — Department Filtreleme

**Aynı pattern.**

#### 2.4 LayoutController — Session Validasyonu

**Sorun:** Session'daki CompanyID yetki kontrolünden geçmiyor.

**Çözüm:**
```csharp
// Index action — session'dan company alındığında yetki kontrolü ekle
var sessionCompanyId = HttpContext.Session.GetInt32("SelectedCompanyId");
if (sessionCompanyId.HasValue)
{
    if (!await _authService.CanAccessCompanyAsync(userId, sessionCompanyId.Value))
    {
        HttpContext.Session.Remove("SelectedCompanyId");
        sessionCompanyId = null;
    }
}
```

---

### Faz 3: Navigasyon Menüsü Rol Bazlı Filtreleme

**Amaç:** Sidebar menüsünde kullanıcının erişemeyeceği sayfaları gizle.

**Değişiklik Dosyası:** `Views/Shared/_Layout.cshtml`

**Kurallar:**
| Menü | SystemAdmin | CompanyAdmin | DepartmentManager | Editor | Viewer |
|------|:-----------:|:------------:|:-----------------:|:------:|:------:|
| Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ |
| Companies | ✅ | ❌ | ❌ | ❌ | ❌ |
| Departments | ✅ | ✅ | ✅ | ✅ | ✅ |
| Users | ✅ | ✅ | ❌ | ❌ | ❌ |
| Pages | ✅ | ✅ | ✅ | ✅ | ✅ |
| Layouts | ✅ | ✅ | ❌ | ❌ | ❌ |
| Media Library | ✅ | ✅ | ✅ | ✅ | ✅ |
| Schedules | ✅ | ✅ | ✅ | ✅ | ✅ |

**Implementasyon:** `_Layout.cshtml`'de ViewComponent veya claim bazlı kontrol:
```razor
@{
    var isSystemAdmin = User.FindFirst("IsSystemAdmin")?.Value == "True";
    // CompanyAdmin kontrolü için ya claim ekle ya da ViewBag'den al
}

@if (isSystemAdmin)
{
    <!-- Companies menü -->
}

@if (isSystemAdmin || ViewBag.IsCompanyAdmin == true)
{
    <!-- Users menü -->
    <!-- Layouts menü -->
}
```

**Gerekli Altyapı:** BaseController'da her request'te kullanıcının en yüksek rolünü belirleyip ViewBag'e ata:
```csharp
// BaseController.OnActionExecuting veya bir ActionFilter
ViewBag.IsSystemAdmin = isSystemAdmin;
ViewBag.IsCompanyAdmin = hasAnyCompanyAdminRole;
ViewBag.IsDepartmentManager = hasAnyDepartmentManagerRole;
```

---

### Faz 4: View'larda Buton Görünürlüğü

**Amaç:** Create/Edit/Delete butonlarını kullanıcının rolüne göre gizle.

**Pattern:** Her view'a gerekli rol bilgisini ViewBag ile geçir.

#### Company Views
```razor
<!-- Company/Index.cshtml — Create butonu sadece SystemAdmin -->
@if (ViewBag.IsSystemAdmin == true)
{
    <a asp-action="Create">New Company</a>
}

<!-- Company/Index.cshtml — Edit/Delete sadece SystemAdmin -->
@if (ViewBag.IsSystemAdmin == true)
{
    <a asp-action="Edit">Edit</a>
    <a asp-action="Delete">Delete</a>
}
```

#### Department Views
```razor
<!-- Create: CompanyAdmin+ -->
<!-- Edit/Delete: CompanyAdmin+ -->
<!-- Details: erişebilen herkes -->
```

#### Page/Content/Schedule Views
```razor
<!-- Create/Edit: CanEdit (DepartmentManager VEYA Editor) -->
<!-- Delete: DepartmentManager only -->
<!-- View: erişebilen herkes -->
```

#### User Views
```razor
<!-- Create/Edit/Delete: SystemAdmin veya CompanyAdmin -->
<!-- ManageRoles: SystemAdmin veya CompanyAdmin -->
<!-- Details: SystemAdmin veya CompanyAdmin -->
```

#### Layout Views
```razor
<!-- Create/Edit/Delete: CompanyAdmin+ -->
<!-- View: CompanyAdmin+ (DeptManager göremez) -->
```

---

### Faz 5: Layout — Departman Bağlamı

**Amaç:** Layout controller'da departman bağlamı ekle. Kullanıcı bir departman seçtiğinde sadece o departmanın şirketine ait layout'lar görünsün.

**Mevcut Durum:** Layout → CompanyID (departman bilgisi yok)

**Yeni Akış:**
1. Kullanıcı departman sayfasına gider (ör: Dept 5, Company A)
2. "Layouts" sekmesine/menüsüne geçer
3. Sadece Company A'nın layout'ları görünür
4. Departman bağlamı korunur (breadcrumb: Company A > Dept 5 > Layouts)

**Değişiklikler:**
- LayoutController.Index'e `departmentId` parametresi ekle (opsiyonel)
- DepartmentID verilirse → department'ın CompanyID'sini bul → o şirketin layout'larını göster
- CompanyID verilirse → o şirketin layout'larını göster (mevcut davranış)
- Hiçbiri yoksa → session veya kullanıcının ilk şirketi

```csharp
public async Task<IActionResult> Index(int? companyId, int? departmentId)
{
    var userId = GetCurrentUserId();
    int resolvedCompanyId;

    if (departmentId.HasValue)
    {
        var dept = await _departmentService.GetByIdAsync(departmentId.Value);
        if (dept == null || !await _authService.CanAccessDepartmentAsync(userId, departmentId.Value))
            return AccessDenied();
        resolvedCompanyId = dept.CompanyID;
        ViewBag.DepartmentContext = dept;
    }
    else if (companyId.HasValue && companyId.Value > 0)
    {
        if (!await _authService.CanAccessCompanyAsync(userId, companyId.Value))
            return AccessDenied();
        resolvedCompanyId = companyId.Value;
    }
    else
    {
        // session veya kullanıcının ilk erişebildiği şirket
        ...
    }

    var layouts = await _layoutService.GetByCompanyIdAsync(resolvedCompanyId);
    // ...
}
```

---

### Faz 6: Veri Bütünlüğü Kuralları

#### 6.1 DepartmentRole atanırken CompanyRole zorunlu

**Kural:** `AssignDepartmentRoleAsync` çağrıldığında, kullanıcının o departmanın şirketinde CompanyRole'ü yoksa otomatik oluştur.

```csharp
public async Task AssignDepartmentRoleAsync(int userId, int departmentId, string role, string assignedBy)
{
    var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
    if (department == null) throw new ArgumentException("Department not found");

    // CompanyRole yoksa otomatik "Viewer" olarak ata
    var companyRole = await GetCompanyRoleAsync(userId, department.CompanyID);
    if (companyRole == null)
    {
        await AssignCompanyRoleAsync(userId, department.CompanyID, "Viewer", assignedBy);
    }

    // Mevcut department role mantığı devam eder...
}
```

#### 6.2 CompanyAdmin kendi rolünü kaldıramaz

```csharp
public async Task RemoveCompanyRoleAsync(int userId, int companyId, int currentUserId)
{
    // Kendi rolünü kaldırmak istiyorsa engelle
    if (userId == currentUserId)
    {
        throw new InvalidOperationException("Cannot remove your own company role");
    }
    // Mevcut mantık...
}
```

**Not:** `RemoveCompanyRoleAsync`'e `currentUserId` parametresi eklenmeli veya controller'da kontrol yapılmalı.

#### 6.3 CompanyRole silindiğinde DepartmentRole'ler de temizlensin

```csharp
public async Task RemoveCompanyRoleAsync(int userId, int companyId)
{
    // Önce o şirketteki tüm department rollerini kaldır
    var departments = await _unitOfWork.Departments.FindAsync(d => d.CompanyID == companyId);
    foreach (var dept in departments)
    {
        var deptRole = await GetDepartmentRoleAsync(userId, dept.DepartmentID);
        if (deptRole != null)
        {
            await _unitOfWork.UserDepartmentRoles.DeleteAsync(deptRole.UserDepartmentRoleID);
        }
    }

    // Sonra company role'ü kaldır
    var role = await GetCompanyRoleAsync(userId, companyId);
    if (role != null)
    {
        await _unitOfWork.UserCompanyRoles.DeleteAsync(role.UserCompanyRoleID);
        await _unitOfWork.SaveChangesAsync();
        ClearUserCache(userId);
    }
}
```

---

### Faz 7: Dashboard Scope'lama

**Amaç:** Dashboard'daki istatistikleri kullanıcının erişebildiği veriye göre scope'la.

**Değişiklikler:**
- HomeController.Index → AuthService'den kullanıcının şirket/departman sayılarını al
- Quick Actions → Rol bazlı göster/gizle:
  - "Add Company" → sadece SystemAdmin
  - "Upload Content" → DepartmentManager + Editor
  - "Create Page" → DepartmentManager + Editor
  - "Schedule Playlist" → DepartmentManager + Editor

---

## Özet — Faz Sıralaması

| Faz | Kapsam | Öncelik | Tahmini Dosya Sayısı |
|-----|--------|---------|---------------------|
| **Faz 1** | Editor rolü tanımlama | Yüksek | 4 dosya (IAuthorizationService, AuthorizationService, ContentController, ScheduleController, PageController) |
| **Faz 2** | Veri filtreleme düzeltmeleri | **KRİTİK** | 5 dosya (PageController, ContentController, ScheduleController, LayoutController + ilgili view'lar) |
| **Faz 3** | Navigasyon menüsü filtreleme | Yüksek | 2 dosya (_Layout.cshtml, BaseController) |
| **Faz 4** | View buton görünürlüğü | Orta | 15+ view dosyası |
| **Faz 5** | Layout departman bağlamı | Orta | 2 dosya (LayoutController, Layout/Index.cshtml) |
| **Faz 6** | Veri bütünlüğü kuralları | Yüksek | 2 dosya (AuthorizationService, UserController) |
| **Faz 7** | Dashboard scope'lama | Düşük | 2 dosya (HomeController, Home/Index.cshtml) |

---

## Bağımlılıklar

```
Faz 1 (Editor Rolü)
  ↓
Faz 2 (Veri Filtreleme) ← Bu önce yapılmalı çünkü güvenlik açığı
  ↓
Faz 3 (Navigasyon) ← BaseController'da rol bilgisi gerekiyor
  ↓
Faz 4 (Buton Görünürlüğü) ← Faz 3'ten ViewBag değerlerini kullanır
  ↓
Faz 5 (Layout Bağlamı)
  ↓
Faz 6 (Veri Bütünlüğü)
  ↓
Faz 7 (Dashboard)
```

**Önerilen başlama sırası:** Faz 2 → Faz 1 → Faz 3 → Faz 6 → Faz 4 → Faz 5 → Faz 7
