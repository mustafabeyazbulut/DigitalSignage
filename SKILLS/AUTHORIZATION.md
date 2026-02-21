# Yetkilendirme Sistemi (Authorization)

## Genel Bakış

Sistem üç kademeli hiyerarşik yetkilendirme modeli kullanır. Hiçbir rolü olmayan kullanıcılar yalnızca Dashboard'u görebilir.

## Rol Hiyerarşisi

```
SystemAdmin          → Tüm sistem erişimi
  └─ CompanyAdmin    → Şirket geneli erişim (atandığı şirketler)
       └─ Department Rolleri (atandığı departmanlar)
            ├─ DepartmentManager  → Departmanda tam yetki (CRUD + silme)
            ├─ Editor             → Departmanda oluşturma/düzenleme
            └─ Viewer             → Departmanda salt okunur erişim
```

## Erişim Matrisi

| Kaynak | SystemAdmin | CompanyAdmin | Manager | Editor | Viewer | Rolsüz |
|---|---|---|---|---|---|---|
| Dashboard | Tam | Tam | Kısıtlı | Kısıtlı | Kısıtlı | Sadece karşılama |
| Companies (CRUD) | Tam | - | - | - | - | - |
| Departments (Liste) | Tümü | Şirketin tümü | Atananlar | Atananlar | Atananlar | Erişim yok |
| Departments (Oluştur/Düzenle/Sil) | Evet | Evet | - | - | - | - |
| Users (CRUD) | Tam | Şirket kapsamlı | - | - | - | - |
| Layouts (Liste) | Tümü | Şirketin tümü | - | - | - | - |
| Layouts (Oluştur/Düzenle/Sil) | Evet | Evet | - | - | - | - |
| Pages (Liste) | Tümü | Şirketin tümü | Departmanın | Departmanın | Departmanın | Erişim yok |
| Pages (Oluştur/Düzenle) | Evet | Evet | Evet | Evet | - | - |
| Pages (Sil) | Evet | Evet | Evet | - | - | - |
| Content (Liste) | Tümü | Şirketin tümü | Departmanın | Departmanın | Departmanın | Erişim yok |
| Content (Oluştur/Düzenle) | Evet | Evet | Evet | Evet | - | - |
| Content (Sil) | Evet | Evet | Evet | - | - | - |
| Schedules (Liste) | Tümü | Şirketin tümü | Departmanın | Departmanın | Departmanın | Erişim yok |
| Schedules (Oluştur/Düzenle) | Evet | Evet | Evet | Evet | - | - |
| Schedules (Sil) | Evet | Evet | Evet | - | - | - |

## Navigasyon Menüsü Görünürlüğü

| Menü Öğesi | Görünürlük Koşulu |
|---|---|
| Dashboard | Her zaman (tüm kullanıcılar) |
| **Yönetim Bölümü** | `HasAnyRole == true` |
| Companies | `IsSystemAdmin == true` |
| Departments | `HasAnyRole == true` |
| Users | `IsSystemAdmin \|\| IsCompanyAdmin` |
| **İçerik & Tasarım Bölümü** | `HasAnyRole == true` |
| Pages | `HasAnyRole == true` |
| Layouts | `IsSystemAdmin \|\| IsCompanyAdmin` |
| Media Library | `HasAnyRole == true` |
| Schedules | `HasAnyRole == true` |

## Rolsüz Kullanıcı Davranışı

Hiçbir rol ataması olmayan (SystemAdmin değil, CompanyRole yok, DepartmentRole yok) kullanıcılar:

1. **Dashboard:** Yalnızca karşılama mesajı ve "Erişim Atanmamış" bilgilendirmesi görür. İstatistik kartları, aktivite tablosu ve hızlı aksiyonlar gizlenir.
2. **Navigasyon:** Yalnızca Dashboard linki görünür. Yönetim ve İçerik & Tasarım bölümleri tamamen gizlenir.
3. **URL ile doğrudan erişim:** Controller'lardaki `HasAnyRoleAsync` guard'ı ile `AccessDenied` sayfasına yönlendirilir.
4. **Profil/Ayarlar:** Erişilebilir (kendi hesap bilgileri).

## Uygulama Detayları

### AuthorizationService Metotları

```
HasAnyRoleAsync(userId)              → Kullanıcının herhangi bir rolü var mı?
IsSystemAdminAsync(userId)           → SystemAdmin mi?
HasAnyCompanyAdminRoleAsync(userId)  → Herhangi bir şirkette CompanyAdmin mi?
IsCompanyAdminAsync(userId, companyId)    → Belirli şirkette CompanyAdmin mi?
CanAccessCompanyAsync(userId, companyId)  → Şirkete erişimi var mı?
CanAccessDepartmentAsync(userId, deptId)  → Departmana erişimi var mı?
CanEditInDepartmentAsync(userId, deptId)  → Departmanda düzenleme yetkisi var mı? (Editor+)
IsDepartmentManagerAsync(userId, deptId)  → Departman Yöneticisi mi? (Manager+)
CanAccessPageAsync(userId, pageId)        → Sayfaya erişimi var mı?
CanEditPageAsync(userId, pageId)          → Sayfayı düzenleyebilir mi?
CanModifyPageAsync(userId, pageId)        → Sayfayı silebilir mi?
```

### BaseController ViewBag Değerleri

Her request'te `SetupRoleFlags()` tarafından ayarlanır:

| ViewBag | Tip | Açıklama |
|---|---|---|
| `IsSystemAdmin` | bool | SystemAdmin flag'i |
| `IsCompanyAdmin` | bool | Herhangi bir şirkette CompanyAdmin |
| `HasAnyRole` | bool | Herhangi bir rol ataması var mı |

### Controller Guard Paterni

Her controller'ın Index ve Create action'larında:

```csharp
var userId = GetCurrentUserId();
if (!await _authService.HasAnyRoleAsync(userId))
    return AccessDenied();
```

### Departman Bazlı Buton Görünürlüğü

Page/Content/Schedule Index view'larında departman bağlamı varsa controller'dan gelen izinler kullanılır:

```csharp
// Controller'da
ViewBag.CanEdit = await _authService.CanEditInDepartmentAsync(userId, departmentId.Value);
ViewBag.CanDelete = await _authService.IsDepartmentManagerAsync(userId, departmentId.Value);

// View'da
@if (ViewBag.CanEdit == true) { /* Edit butonu */ }
@if (ViewBag.CanDelete == true) { /* Delete butonu */ }
```

## Veri İzolasyonu Kuralları

- **Şirket seçimi:** Üst header'daki dropdown ile yapılır, session'a yazılır (`SelectedCompanyId`)
- **Departman/Page/Content/Schedule dropdown'ları:** Session'daki seçili şirkete göre filtrelenir
- **Create/Edit formlarında:** Company seçim alanı yoktur; üstten seçili şirket otomatik kullanılır
- **CompanyID:** Hidden field olarak form'da taşınır

## Rol Atama Kuralları

1. **DepartmentRole atanırken** CompanyRole yoksa otomatik olarak `Viewer` CompanyRole oluşturulur
2. **CompanyRole silinirken** o şirketteki tüm DepartmentRole'ler cascade olarak silinir
3. **SystemAdmin kullanıcılara** CompanyRole/DepartmentRole atanamaz
4. **CompanyAdmin** kullanıcıları SystemAdmin yapamaz
5. **Kullanıcı kendini** silemez, devre dışı bırakamaz, kendi IsSystemAdmin flag'ini değiştiremez
6. **Son aktif SystemAdmin** silinemez ve devre dışı bırakılamaz

## Cache Stratejisi

| Cache Key | TTL | Açıklama |
|---|---|---|
| `user_sysadmin_{userId}` | 15 dk | SystemAdmin kontrolü |
| `user_{userId}_company_{companyId}_access` | 10 dk | Şirket erişim kontrolü |

Rol değişikliklerinde `ClearUserCache(userId)` çağrılır ve ilgili kullanıcının tüm cache key'leri temizlenir.
