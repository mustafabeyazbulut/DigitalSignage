# Rol Kombinasyonları ve İhtimaller

## Yapı Taşları

**System Level** (`User.IsSystemAdmin`): `true` / `false`

**Company Level** (`UserCompanyRole.Role`) — her şirket için ayrı:
- Rol yok (kayıt yok = erişim yok)
- `"CompanyAdmin"`
- `"Viewer"`

**Department Level** (`UserDepartmentRole.Role`) — her departman için ayrı:
- Rol yok (kayıt yok = erişim yok)
- `"DepartmentManager"`
- `"Editor"`
- `"Viewer"`

---

## Tüm Anlamlı Kombinasyonlar

### 1. Saf SystemAdmin
- `IsSystemAdmin = true`, başka role gerek yok
- Her şeye tam erişim (tüm şirketler, departmanlar, sayfalar, içerikler)
- User yönetimi dahil

### 2. Tek Şirkette CompanyAdmin
- `CompanyAdmin(A)`
- A'nın TÜM departmanlarına erişir
- Sayfa/Content/Layout/Schedule CRUD yapabilir
- A'daki kullanıcıları yönetir, rol atar
- B şirketine erişemez

### 3. Birden Fazla Şirkette CompanyAdmin
- `CompanyAdmin(A) + CompanyAdmin(B)`
- Her iki şirkette tam departman yetkisi
- User yönetiminde HER İKİ şirketin kullanıcılarını görür
- C şirketine erişemez

### 4. Tek Şirkette Viewer (Company seviyesinde)
- `Viewer(A)`
- A şirketini görebilir (listede çıkar)
- **AMA** hiçbir departmana erişemez (doc Test 4)
- Pratikte neredeyse işe yaramaz — sadece şirketin var olduğunu görür

### 5. CompanyAdmin + Viewer (farklı şirketlerde)
- `CompanyAdmin(A) + Viewer(B)`
- A'da tam yetki
- B'yi görebilir ama B'nin departmanlarına erişemez
- User yönetiminde SADECE A'nın kullanıcılarını yönetir

### 6. Company Viewer + DepartmentManager (aynı şirkette)
- `Viewer(A) + DepartmentManager(A/Dept1)`
- A şirketini görebilir
- Dept1'i tam yönetir (sayfa, content, schedule CRUD)
- Dept2'ye erişemez
- Departman oluşturamaz/silemez (CompanyAdmin değil)
- User yönetimine erişemez

### 7. Company Viewer + Editor (aynı şirkette)
- `Viewer(A) + Editor(A/Dept1)`
- A şirketini görebilir
- Dept1'e erişebilir (görüntüleme)
- **AMA** `IsDepartmentManagerAsync` sadece `"DepartmentManager"` kontrol ediyor
- Editor pratikte Viewer ile AYNI yetkiye sahip — yazma işlemi yapamaz

### 8. Company Viewer + Karışık Departman Rolleri
- `Viewer(A) + DepartmentManager(A/Dept1) + Editor(A/Dept2) + Viewer(A/Dept3)`
- Dept1: tam yönetim
- Dept2: sadece okuma (Editor ama yazma yetkisi yok)
- Dept3: sadece okuma

### 9. CompanyAdmin(A) + Başka Şirkette DepartmentManager
- `CompanyAdmin(A) + Viewer(B) + DepartmentManager(B/Dept5)`
- A'da her şey tam
- B'de sadece Dept5'i yönetir
- B'de departman oluşturamaz

### 10. Çok Şirkette Karışık Roller
- `CompanyAdmin(A) + CompanyAdmin(B) + Viewer(C) + DepartmentManager(C/Dept9)`
- A ve B'de tam yetki
- C'de sadece Dept9

### 11. DepartmentManager AMA Company Role YOK (Edge Case!)
- `DepartmentManager(A/Dept1)` ama `UserCompanyRole` kaydı yok
- `CanAccessDepartmentAsync` → **EVET** (UserDepartmentRole var)
- `CanAccessCompanyAsync` → **HAYIR** (UserCompanyRole yok)
- Departman içeriğini görebilir ama şirket sayfalarına erişemez
- **Tutarsız durum — veri bütünlüğü sorunu**

### 12. SystemAdmin + Herhangi Bir Rol (Redundant)
- `IsSystemAdmin=true + CompanyAdmin(A) + DepartmentManager(B/Dept1)`
- Hepsi gereksiz, SystemAdmin zaten her şeye erişir
- Mevcut kodda SystemAdmin'e rol atanmasını engelliyoruz

---

## Tespit Edilen Problemler

| # | Problem | Kategori |
|---|---------|----------|
| 1 | **Editor rolü pratikte Viewer ile aynı** — `IsDepartmentManagerAsync` sadece `"DepartmentManager"` kontrol ediyor | Tasarım boşluğu |
| 2 | **DepartmentRole var ama CompanyRole yok** — tutarsız erişim, enforce edilmiyor | Veri bütünlüğü |
| 3 | **CompanyAdmin kendi rolünü kaldırabilir** — self-check yok | Güvenlik riski |
| 4 | **Company Viewer pratikte işe yaramaz** — departmanlara erişemez | UX sorunu |
