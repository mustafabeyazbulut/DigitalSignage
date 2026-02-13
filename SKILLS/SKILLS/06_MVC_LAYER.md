# MVC Controllers & Views

## Genel Bakış

ASP.NET Core 9 MVC'de Controllers HTTP isteklerini işler ve Views ise HTML'i render eder. Model Binding ve Action Results kullanılarak veri akışı sağlanır.

---

## Controller Architecture

### Base Controller

```csharp
public class BaseController : Controller
{
    protected readonly ILogger<BaseController> _logger;

    public BaseController(ILogger<BaseController> logger)
    {
        _logger = logger;
    }

    protected IActionResult HandleException(Exception ex)
    {
        _logger.LogError(ex, "An error occurred");

        if (ex is ValidationException)
            return BadRequest(new { error = ex.Message });

        return StatusCode(500, new { error = "Internal server error" });
    }

    protected User GetCurrentUser()
    {
        return new User { UserID = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0") };
    }
}
```

---

## User Controller

### IUserController Actions

```csharp
public interface IUserController
{
    // Display
    Task<IActionResult> Index();
    Task<IActionResult> Details(int id);

    // Create
    Task<IActionResult> Create();
    [HttpPost]
    Task<IActionResult> Create(CreateUserDTO dto);

    // Edit
    Task<IActionResult> Edit(int id);
    [HttpPost]
    Task<IActionResult> Edit(int id, UserViewModel vm);

    // Delete
    Task<IActionResult> Delete(int id);
    [HttpPost, ActionName("Delete")]
    Task<IActionResult> DeleteConfirmed(int id);
}
```

### UserController Implementation

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : BaseController
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UserController(
        IUserService userService,
        IMapper mapper,
        ILogger<UserController> logger) : base(logger)
    {
        _userService = userService;
        _mapper = mapper;
    }

    /// <summary>
    /// Tüm kullanıcıları listele
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(int page = 1)
    {
        try
        {
            const int pageSize = 10;
            var users = await _userService.GetAllAsync();
            var pagedUsers = users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = _mapper.Map<List<UserViewModel>>(pagedUsers);

            return View(viewModel);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Kullanıcı detaylarını görüntüle
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound("User not found");

            var viewModel = _mapper.Map<UserViewModel>(user);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Yeni kullanıcı oluştur (GET)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Yeni kullanıcı oluştur (POST)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateUserDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = await _userService.CreateUserAsync(dto);

            TempData["Success"] = $"User {user.UserName} created successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Kullanıcıyı düzenle (GET)
    /// </summary>
    [HttpGet("{id}/edit")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var viewModel = _mapper.Map<UserViewModel>(user);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Kullanıcıyı güncelle (POST)
    /// </summary>
    [HttpPost("{id}/edit")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, UserViewModel viewModel)
    {
        try
        {
            if (id != viewModel.UserID)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return View(viewModel);

            var user = _mapper.Map<User>(viewModel);
            await _userService.UpdateAsync(user);

            TempData["Success"] = "User updated successfully";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Kullanıcıyı sil (GET)
    /// </summary>
    [HttpGet("{id}/delete")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            var viewModel = _mapper.Map<UserViewModel>(user);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Kullanıcıyı sil (POST)
    /// </summary>
    [HttpPost("{id}/delete"), ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _userService.DeleteAsync(id);

            TempData["Success"] = "User deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Şifreyi değiştir
    /// </summary>
    [HttpPost("{id}/change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            var success = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);

            if (!success)
                return BadRequest("Current password is incorrect");

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
```

---

## Company Controller

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CompanyController : BaseController
{
    private readonly ICompanyService _companyService;
    private readonly IMapper _mapper;

    public CompanyController(
        ICompanyService companyService,
        IMapper mapper,
        ILogger<CompanyController> logger) : base(logger)
    {
        _companyService = companyService;
        _mapper = mapper;
    }

    /// <summary>
    /// Tüm şirketleri listele
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var companies = await _companyService.GetAllAsync();
            var viewModel = _mapper.Map<List<CompanyViewModel>>(companies);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Şirket detaylarını görüntüle
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var viewModel = await _companyService.GetCompanyDetailsAsync(id);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Yeni şirket oluştur (GET)
    /// </summary>
    [HttpGet("create")]
    [Authorize(Roles = "SystemAdmin")]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Yeni şirket oluştur (POST)
    /// </summary>
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Create(CompanyCreateDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return View(dto);

            dto.CreatedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            var company = await _companyService.CreateCompanyAsync(dto);

            TempData["Success"] = "Company created successfully";
            return RedirectToAction(nameof(Details), new { id = company.CompanyID });
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Şirketi güncelle
    /// </summary>
    [HttpPost("{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> Update(int id, CompanyUpdateDTO dto)
    {
        try
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound();

            company.CompanyName = dto.CompanyName;
            company.Description = dto.Description;
            company.IsActive = dto.IsActive;
            company.ModifiedBy = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";

            await _companyService.UpdateAsync(company);

            return Ok(new { message = "Company updated successfully" });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
```

---

## Layout Controller

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LayoutController : BaseController
{
    private readonly ILayoutService _layoutService;
    private readonly IMapper _mapper;

    public LayoutController(
        ILayoutService layoutService,
        IMapper mapper,
        ILogger<LayoutController> logger) : base(logger)
    {
        _layoutService = layoutService;
        _mapper = mapper;
    }

    /// <summary>
    /// Dinamik layout oluştur
    /// </summary>
    [HttpPost("create-dynamic")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDynamicLayout([FromBody] DynamicLayoutDTO dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var layout = await _layoutService.CreateDynamicLayoutAsync(dto);
            return CreatedAtAction(nameof(GetDynamicLayout), new { id = layout.LayoutID }, layout);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Dinamik layout detaylarını getir
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDynamicLayout(int id)
    {
        try
        {
            var layout = await _layoutService.GetDynamicLayoutAsync(id);
            return Ok(layout);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Şirketteki tüm layoutları getir
    /// </summary>
    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetCompanyLayouts(int companyId)
    {
        try
        {
            var layouts = await _layoutService.GetCompanyLayoutsAsync(companyId);
            var viewModels = _mapper.Map<List<LayoutViewModel>>(layouts);
            return Ok(viewModels);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
```

---

## Routing Configuration

### Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

var app = builder.Build();

// Configure middleware
app.UseAuthentication();
app.UseAuthorization();

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
```

---

## View Components

### LayoutGridComponent

```csharp
public class LayoutGridViewComponent : ViewComponent
{
    private readonly ILayoutService _layoutService;

    public LayoutGridViewComponent(ILayoutService layoutService)
    {
        _layoutService = layoutService;
    }

    public async Task<IViewComponentResult> InvokeAsync(int layoutId)
    {
        var layout = await _layoutService.GetDynamicLayoutAsync(layoutId);
        return View(layout);
    }
}
```

### LayoutGridComponent.cshtml

```html
@model DynamicLayoutViewModel

<div class="layout-grid" style="display: grid;
    grid-template-columns: repeat(@Model.GridColumnsX, 1fr);
    grid-template-rows: repeat(@Model.GridRowsY, 1fr);
    gap: 10px;
    height: 100%;">

    @foreach (var section in Model.Sections.OrderBy(s => s.Row).ThenBy(s => s.Column))
    {
        <div class="grid-section" style="
            grid-column: @(section.Column + 1);
            grid-row: @(section.Row + 1);
            background-color: #f0f0f0;
            border: 1px solid #ddd;
            padding: 10px;">
            <p>@section.Position</p>
            <p>Content for Section @section.SectionID</p>
        </div>
    }
</div>
```

---

## Razor View Examples

### List View (Users/Index.cshtml)

```html
@model List<UserViewModel>

@{
    ViewData["Title"] = "Users";
}

<div class="container mt-5">
    <div class="row mb-4">
        <div class="col-md-8">
            <h1>Users</h1>
        </div>
        <div class="col-md-4 text-right">
            <a asp-action="Create" class="btn btn-primary">
                <i class="bi bi-plus"></i> New User
            </a>
        </div>
    </div>

    <table class="table table-striped">
        <thead>
            <tr>
                <th>User Name</th>
                <th>Email</th>
                <th>Full Name</th>
                <th>Status</th>
                <th>Created Date</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var user in Model)
            {
                <tr>
                    <td>@user.UserName</td>
                    <td>@user.Email</td>
                    <td>@user.FullName</td>
                    <td>
                        <span class="badge @(user.IsActive ? "bg-success" : "bg-danger")">
                            @user.StatusText
                        </span>
                    </td>
                    <td>@user.CreatedDate.ToString("yyyy-MM-dd")</td>
                    <td>
                        <a asp-action="Details" asp-route-id="@user.UserID" class="btn btn-sm btn-info">View</a>
                        <a asp-action="Edit" asp-route-id="@user.UserID" class="btn btn-sm btn-warning">Edit</a>
                        <a asp-action="Delete" asp-route-id="@user.UserID" class="btn btn-sm btn-danger">Delete</a>
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>
```

### Form View (Users/Create.cshtml)

```html
@model CreateUserDTO

@{
    ViewData["Title"] = "Create User";
}

<div class="container mt-5">
    <div class="row">
        <div class="col-md-8 offset-md-2">
            <h1>Create New User</h1>

            <form asp-action="Create" method="post">
                @if (!ViewData.ModelState.IsValid)
                {
                    <div class="alert alert-danger">
                        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                        <strong>Error!</strong>
                        <ul>
                            @foreach (var modelState in ViewData.ModelState.Values)
                            {
                                @foreach (var error in modelState.Errors)
                                {
                                    <li>@error.ErrorMessage</li>
                                }
                            }
                        </ul>
                    </div>
                }

                <div class="mb-3">
                    <label asp-for="UserName" class="form-label"></label>
                    <input asp-for="UserName" class="form-control" />
                    <span asp-validation-for="UserName" class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label asp-for="Email" class="form-label"></label>
                    <input asp-for="Email" type="email" class="form-control" />
                    <span asp-validation-for="Email" class="text-danger"></span>
                </div>

                <div class="mb-3">
                    <label asp-for="Password" class="form-label"></label>
                    <input asp-for="Password" type="password" class="form-control" />
                    <span asp-validation-for="Password" class="text-danger"></span>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label asp-for="FirstName" class="form-label"></label>
                            <input asp-for="FirstName" class="form-control" />
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="mb-3">
                            <label asp-for="LastName" class="form-label"></label>
                            <input asp-for="LastName" class="form-control" />
                        </div>
                    </div>
                </div>

                <div class="form-group">
                    <button type="submit" class="btn btn-primary">Create</button>
                    <a asp-action="Index" class="btn btn-secondary">Cancel</a>
                </div>
            </form>
        </div>
    </div>
</div>
```

---

## Model Binding & Validation

```csharp
[ApiController]
[Route("api/[controller]")]
public class PageController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePageDTO dto,  // JSON body
        [FromServices] IPageService service)
    {
        // ModelState validation otomatik
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var page = await service.CreatePageAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = page.PageID }, page);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        [FromRoute] int id,  // URL parameter
        [FromServices] IPageService service)
    {
        var page = await service.GetByIdAsync(id);
        if (page == null)
            return NotFound();

        return Ok(page);
    }
}
```

---

## Smart Back Navigation Sistemi

### Genel Bakış

Digital Signage projesi, kullanıcı deneyimini iyileştirmek için profesyonel bir **Smart Back Navigation** sistemi kullanır. Bu sistem, sayfalar arası geri dönüş işlemlerini akıllıca yöneterek alakasız sayfalara yönlendirme problemini çözer.

### Mimari

Smart Back Navigation sistemi 3 katmandan oluşur:

1. **JavaScript NavigationHelper** (`wwwroot/js/site.js`)
2. **BaseController Smart Logic** (`Controllers/BaseController.cs`)
3. **View Integration** (26+ view dosyası)

---

### 1. NavigationHelper (JavaScript)

**Konum:** `wwwroot/js/site.js`

#### Özellikler:
- **Session-based tracking**: Son 10 sayfayı `sessionStorage`'da takip eder
- **Browser history integration**: `history.back()` API'sini güvenli kullanır
- **Same-domain security**: XSS koruması için aynı domain kontrolü
- **Intelligent fallback**: History yoksa context-aware URL'lere yönlendirir

#### Kullanım:

```javascript
// Otomatik initialization (DOM ready)
NavigationHelper.init();

// Manuel kullanım
NavigationHelper.goBack('/fallback-url');

// Geçmiş kontrolü
if (NavigationHelper.canGoBack()) {
    NavigationHelper.goBack();
}
```

#### Örnek Kod:

```javascript
const NavigationHelper = {
    init: function () {
        this.trackPageVisit();
        this.attachEventListeners();
    },

    trackPageVisit: function () {
        let navHistory = JSON.parse(sessionStorage.getItem('navHistory') || '[]');
        navHistory.push({
            url: window.location.href,
            path: window.location.pathname,
            timestamp: Date.now()
        });
        navHistory = navHistory.slice(-10); // Son 10 sayfa
        sessionStorage.setItem('navHistory', JSON.stringify(navHistory));
    },

    goBack: function (fallbackUrl) {
        // 1. returnUrl parametresi (en yüksek öncelik)
        const urlParams = new URLSearchParams(window.location.search);
        const returnUrl = urlParams.get('returnUrl');
        if (returnUrl) {
            window.location.href = returnUrl;
            return;
        }

        // 2. Breadcrumb-aware fallback (ikinci öncelik)
        if (fallbackUrl && fallbackUrl !== window.location.pathname) {
            window.location.href = fallbackUrl;
            return;
        }

        // 3. Browser history (son çare)
        if (window.history.length > 1 && this.isSameDomain(document.referrer)) {
            window.history.back();
            return;
        }

        // 4. Default fallback
        window.location.href = fallbackUrl || '/';
    }
};
```

---

### 2. BaseController Smart Logic

**Konum:** `Controllers/BaseController.cs`

#### SetupSmartBackNavigation Metodu

Her action execute edilmeden önce `OnActionExecuting` içinde çağrılır:

```csharp
private void SetupSmartBackNavigation(ActionExecutingContext context)
{
    var controllerName = context.RouteData.Values["controller"]?.ToString();
    var actionName = context.RouteData.Values["action"]?.ToString();

    string defaultFallback = "/";

    // Context-aware fallback URL belirleme
    if (actionName == "Create" || actionName == "Edit" || actionName == "Details")
    {
        defaultFallback = $"/{controllerName}/Index";
    }
    else if (controllerName == "Account" &&
            (actionName == "Profile" || actionName == "Settings" || actionName == "ChangePassword"))
    {
        defaultFallback = "/Home/Index";
    }
    else if (actionName != "Index")
    {
        defaultFallback = $"/{controllerName}/Index";
    }

    // ViewBag'e aktar
    ViewBag.ReturnUrl = Request.Headers["Referer"].ToString();
    ViewBag.DefaultReturnUrl = defaultFallback;
    ViewBag.CurrentUrl = $"{Request.Path}{Request.QueryString}";
}
```

#### Fallback URL Mantığı

| Sayfa Tipi | Fallback Hedefi | Örnek |
|------------|----------------|-------|
| Create | Controller Index | `/Company/Create` → `/Company/Index` |
| Edit | Controller Index | `/User/Edit/5` → `/User/Index` |
| Details | Controller Index | `/Department/Details/3` → `/Department/Index` |
| Profile | Home | `/Account/Profile` → `/Home/Index` |
| Settings | Home | `/Account/Settings` → `/Home/Index` |
| ChangePassword | Home | `/Account/ChangePassword` → `/Home/Index` |
| Diğer | Controller Index | `/Page/Custom` → `/Page/Index` |

---

### 3. View Integration

#### Smart Back Button Kullanımı

**Yeni Format:**
```html
<button type="button" class="btn btn-secondary" data-smart-back="@ViewBag.DefaultReturnUrl">
    <i class="fas fa-arrow-left me-1"></i>@T("common.back")
</button>
```

**Eski Format (Kullanılmamalı):**
```html
<a asp-action="Index" class="btn btn-secondary">
    <i class="fas fa-arrow-left me-1"></i>@T("common.back")
</a>
```

#### Güncellenen View'lar (26 Dosya)

**Account Views:**
- Profile.cshtml
- Settings.cshtml
- ChangePassword.cshtml
- AccessDenied.cshtml

**CRUD Views (Her controller için Create/Edit/Details):**
- Company (Create, Edit, Details)
- Department (Create, Edit, Details)
- User (Create, Edit, Details)
- Page (Create, Edit, Details)
- Layout (Create, Edit, Details)
- Content (Create, Edit, Details)
- Schedule (Create, Edit, Details)

---

### 4. Çalışma Akışı

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. Kullanıcı "Geri" Butonuna Tıklar                             │
│    <button data-smart-back="/Company/Index">                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. JavaScript Event Listener (site.js)                          │
│    NavigationHelper.goBack('/Company/Index')                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. Browser History Kontrolü                                     │
│    • history.length > 1? ✓                                      │
│    • document.referrer var? ✓                                   │
│    • Same domain? ✓                                             │
└─────────────────────────────────────────────────────────────────┘
                    │                      │
            ✓ EVET │                      │ HAYIR
                    ▼                      ▼
        ┌──────────────────┐    ┌──────────────────────┐
        │ history.back()    │    │ Fallback URL'e git   │
        │ (Browser native)  │    │ window.location.href │
        └──────────────────┘    └──────────────────────┘
```

---

### 5. Güvenlik Özellikleri

#### Same-Domain Check

```javascript
isSameDomain: function(url) {
    if (!url) return false;
    try {
        const referrerDomain = new URL(url).hostname;
        const currentDomain = window.location.hostname;
        return referrerDomain === currentDomain;
    } catch {
        return false;
    }
}
```

**Amaç:** XSS ve phishing saldırılarını önlemek için sadece aynı domain'den gelen referrer'lara güvenilir.

#### Session Storage

- **Kapsam:** Sadece mevcut browser tab
- **Süre:** Tab kapanana kadar
- **Boyut:** Maksimum 10 sayfa (circular buffer)
- **Privacy:** Başka tab'ler göremez

---

### 6. Test Senaryoları

| Senaryo | Beklenen Davranış |
|---------|-------------------|
| Company Index → Create → **Back** | Company Index'e döner |
| User Details → Edit → **Back** | User Details'e döner |
| Dashboard → Profile → **Back** | Dashboard'a döner |
| Direct link (örn: bookmark) → **Back** | Fallback URL'e gider (Index) |
| Yeni sekmede açma → **Back** | Fallback URL'e gider |
| Browser back button | JavaScript ile aynı mantık çalışır |

---

### 7. Avantajlar

✅ **Kullanıcı Deneyimi:** Sezgisel ve tahmin edilebilir navigasyon
✅ **Güvenlik:** Same-domain kontrolü ile XSS koruması
✅ **Performans:** Hafif JavaScript, minimal overhead
✅ **Bakım:** Merkezi mantık, DRY prensibi
✅ **Esneklik:** Context-aware fallback'ler
✅ **Tarayıcı Desteği:** Tüm modern browser'lar

---

### 8. İyileştirme Geçmişi

#### v2.3.0 (13 Şubat 2026) - 4 Advanced Features ✅

- [x] **Breadcrumb Integration**: Navigation path görselleştirme
  - Otomatik hiyerarşi detection (Controller/Action/ID)
  - Localized breadcrumb metinleri
  - Icon desteği ile görsel zenginlik
  - Tüm sayfalarda otomatik görüntüleme

- [x] **Deep Linking**: URL state management ile enhanced back
  - returnUrl parametresi ile context preservation
  - Browser back/forward button desteği
  - Bookmarkable URLs
  - Page refresh sonrası context korunması

- [x] **Mobile Swipe Gestures**: Touch-based navigation
  - Sağa kaydır = geri git (Instagram/Safari gibi)
  - Visual feedback ve threshold detection
  - Edge zone restriction (50px)
  - Smooth animations

- [x] **Navigation Analytics**: Kullanıcı davranış tracking
  - Page view ve navigation flow tracking
  - Back button usage analytics
  - Swipe gesture statistics
  - localStorage persistence
  - Debug console: `NavigationAnalytics.logSummary()`

**Technical Stack:**
- JavaScript: NavigationHelper, SwipeHandler, NavigationAnalytics
- C#: BaseController breadcrumb logic, ViewComponent
- Razor: Breadcrumb partial view
- CSS: Responsive styling, swipe feedback

**Lines of Code Added:** 549 lines
**Files Modified:** 5 files
**Commit:** 5d25a68

#### v2.3.1 (13 Şubat 2026) - Breadcrumb Priority Fix ✅

**Problem:** Details-Edit sayfaları arasında sonsuz döngü oluşuyordu. NavigationHelper.goBack() metodu browser history'yi breadcrumb hiyerarşisinden önce kontrol ediyordu, bu da yanlış navigasyon sırasına neden oluyordu.

**Çözüm:** Navigation priority sıralaması yeniden düzenlendi:

1. **returnUrl parametresi** (en yüksek öncelik)
   - Query string'den gelen returnUrl kullanılır
   - Details→Edit gibi özel geçişlerde context korunur

2. **Breadcrumb fallback** (ikinci öncelik)
   - Breadcrumb hiyerarşisinden hesaplanan parent URL
   - Index→Edit→Back = Index (doğru davranış)

3. **Browser history** (son çare)
   - Sadece breadcrumb fallback yoksa kullanılır
   - Same-domain security kontrolü ile

**Değişiklikler:**

- `BaseController.cs`: returnUrl query parametresi kontrolü eklendi
  ```csharp
  var returnUrl = request.Query["returnUrl"].ToString();
  if (!string.IsNullOrEmpty(returnUrl))
  {
      breadcrumbBackUrl = returnUrl;
  }
  ```

- `NavigationHelper.goBack()`: Priority sıralaması düzeltildi
  ```javascript
  // 1. returnUrl (highest priority)
  // 2. breadcrumb fallback (second priority)
  // 3. browser history (last resort)
  ```

- Details view'larda Edit linklerine returnUrl parametresi eklendi
  ```razor
  <a asp-action="Edit" asp-route-id="@Model.CompanyID"
     asp-route-returnUrl="@Url.Action("Details", new { id = Model.CompanyID })">
  ```

**Sonuç:** Details↔Edit döngüsü çözüldü, breadcrumb-aware navigation tutarlı çalışıyor.

**Files Modified:** 4 files (BaseController.cs, Details.cshtml, Edit.cshtml, site.js)
**Commit:** 1b6ddb4

---

## Referanslar
- [ASP.NET Core Controllers](https://docs.microsoft.com/aspnet/core/mvc/controllers/actions)
- [Routing](https://docs.microsoft.com/aspnet/core/fundamentals/routing)
- [Views and Razor](https://docs.microsoft.com/aspnet/core/mvc/views/overview)
- [History API](https://developer.mozilla.org/en-US/docs/Web/API/History_API)
- [SessionStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/sessionStorage)
