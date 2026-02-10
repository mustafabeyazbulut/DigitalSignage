# Business Logic & Services

## Genel Bakış

Business Logic Layer, iş kurallarını ve kompleks işlemleri yönetir. Repository Pattern'dan farklı olarak, Services verinin nasıl işlendiğini ve validasyonları kontrol eder.

---

## Service Architecture

### Layered Architecture

```
Controllers (User Requests)
    ↓
Services (Business Logic)
    ↓
Repositories (Data Access)
    ↓
Database
```

### Service Interface

```csharp
public interface IService<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

---

## User Service

### IUserService Interface

```csharp
public interface IUserService : IService<User>
{
    Task<User> GetByUserNameAsync(string userName);
    Task<User> GetByEmailAsync(string email);
    Task<bool> AuthenticateAsync(string userName, string password);
    Task<User> CreateUserAsync(CreateUserDTO dto);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<IEnumerable<UserViewModel>> GetActiveUsersAsync();
    Task<IEnumerable<UserCompanyRole>> GetUserCompaniesAsync(int userId);
}
```

### UserService Implementation

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHelper _passwordHelper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IPasswordHelper passwordHelper,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordHelper = passwordHelper;
        _logger = logger;
    }

    public async Task<User> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User> GetByUserNameAsync(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentNullException(nameof(userName));

        return await _userRepository.GetByUserNameAsync(userName);
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<bool> AuthenticateAsync(string userName, string password)
    {
        var user = await GetByUserNameAsync(userName);
        if (user == null || !user.IsActive)
            return false;

        return _passwordHelper.VerifyPassword(password, user.PasswordHash);
    }

    public async Task<User> CreateUserAsync(CreateUserDTO dto)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(dto.UserName))
            throw new ValidationException("User name is required");

        if (!IsValidEmail(dto.Email))
            throw new ValidationException("Invalid email format");

        if (dto.Password.Length < 6)
            throw new ValidationException("Password must be at least 6 characters");

        // Check if user exists
        var existingUser = await GetByUserNameAsync(dto.UserName);
        if (existingUser != null)
            throw new ValidationException("User already exists");

        // Create new user
        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            PasswordHash = _passwordHelper.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedDate = DateTime.UtcNow,
            IsActive = true,
            LastLoginDate = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);
        _logger.LogInformation($"User {dto.UserName} created successfully");

        return createdUser;
    }

    public async Task<User> UpdateAsync(User user)
    {
        await _userRepository.UpdateAsync(user);
        _logger.LogInformation($"User {user.UserName} updated");
        return user;
    }

    public async Task DeleteAsync(int id)
    {
        await _userRepository.DeleteAsync(id);
        _logger.LogInformation($"User {id} deleted");
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await GetByIdAsync(userId);
        if (user == null)
            throw new ValidationException("User not found");

        // Verify current password
        if (!_passwordHelper.VerifyPassword(currentPassword, user.PasswordHash))
            return false;

        // Update password
        user.PasswordHash = _passwordHelper.HashPassword(newPassword);
        user.ModifiedDate = DateTime.UtcNow;
        await UpdateAsync(user);

        return true;
    }

    public async Task<IEnumerable<UserViewModel>> GetActiveUsersAsync()
    {
        var users = await _userRepository.GetActiveUsersAsync();
        return users.Select(u => MapToViewModel(u));
    }

    public async Task<IEnumerable<UserCompanyRole>> GetUserCompaniesAsync(int userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null)
            throw new ValidationException("User not found");

        // Bu bilgi userCompanyRole repository'den gelecek
        return user.UserCompanyRoles;
    }

    private UserViewModel MapToViewModel(User user)
    {
        return new UserViewModel
        {
            UserID = user.UserID,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            CreatedDate = user.CreatedDate
        };
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

---

## Company Service

### ICompanyService Interface

```csharp
public interface ICompanyService : IService<Company>
{
    Task<Company> GetByCompanyCodeAsync(string companyCode);
    Task<Company> CreateCompanyAsync(CompanyCreateDTO dto);
    Task<CompanyViewModel> GetCompanyDetailsAsync(int companyId);
    Task<IEnumerable<Company>> GetCompaniesForUserAsync(int userId);
}
```

### CompanyService Implementation

```csharp
public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ISystemUnitRepository _systemUnitRepository;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(
        ICompanyRepository companyRepository,
        ISystemUnitRepository systemUnitRepository,
        ILogger<CompanyService> logger)
    {
        _companyRepository = companyRepository;
        _systemUnitRepository = systemUnitRepository;
        _logger = logger;
    }

    public async Task<Company> GetByIdAsync(int id)
    {
        return await _companyRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _companyRepository.GetAllAsync();
    }

    public async Task<Company> GetByCompanyCodeAsync(string companyCode)
    {
        if (string.IsNullOrWhiteSpace(companyCode))
            throw new ArgumentNullException(nameof(companyCode));

        return await _companyRepository.FirstOrDefaultAsync(c => c.CompanyCode == companyCode);
    }

    public async Task<Company> CreateCompanyAsync(CompanyCreateDTO dto)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(dto.CompanyName))
            throw new ValidationException("Company name is required");

        if (string.IsNullOrWhiteSpace(dto.CompanyCode))
            throw new ValidationException("Company code is required");

        // Check duplicate code
        var existing = await GetByCompanyCodeAsync(dto.CompanyCode);
        if (existing != null)
            throw new ValidationException("Company code already exists");

        var company = new Company
        {
            CompanyName = dto.CompanyName,
            CompanyCode = dto.CompanyCode,
            Description = dto.Description,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = dto.CreatedBy
        };

        var created = await _companyRepository.AddAsync(company);
        _logger.LogInformation($"Company {dto.CompanyName} created with ID {created.CompanyID}");

        return created;
    }

    public async Task<Company> UpdateAsync(Company company)
    {
        company.ModifiedDate = DateTime.UtcNow;
        await _companyRepository.UpdateAsync(company);
        return company;
    }

    public async Task DeleteAsync(int id)
    {
        await _companyRepository.DeleteAsync(id);
        _logger.LogInformation($"Company {id} deleted");
    }

    public async Task<CompanyViewModel> GetCompanyDetailsAsync(int companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId);
        if (company == null)
            throw new ValidationException("Company not found");

        var systemCount = await _systemUnitRepository.CountAsync(s => s.CompanyID == companyId);

        return new CompanyViewModel
        {
            CompanyID = company.CompanyID,
            CompanyName = company.CompanyName,
            CompanyCode = company.CompanyCode,
            IsActive = company.IsActive,
            SystemCount = systemCount,
            CreatedDate = company.CreatedDate
        };
    }

    public async Task<IEnumerable<Company>> GetCompaniesForUserAsync(int userId)
    {
        // User'ın UserCompanyRole'larından şirketleri getir
        return await _companyRepository.FindAsync(c =>
            c.UserCompanyRoles.Any(ucr => ucr.UserID == userId && ucr.IsActive)
        );
    }
}
```

---

## Page Service

### IPageService Interface

```csharp
public interface IPageService : IService<Page>
{
    Task<Page> CreatePageAsync(CreatePageDTO dto);
    Task<PageViewModel> GetPageDetailsAsync(int pageId);
    Task<IEnumerable<Page>> GetPagesByDepartmentAsync(int departmentId);
    Task<Page> GetPageWithLayoutAsync(int pageId);
}
```

### PageService Implementation

```csharp
public class PageService : IPageService
{
    private readonly IPageRepository _pageRepository;
    private readonly ILayoutRepository _layoutRepository;
    private readonly ILogger<PageService> _logger;

    public PageService(
        IPageRepository pageRepository,
        ILayoutRepository layoutRepository,
        ILogger<PageService> logger)
    {
        _pageRepository = pageRepository;
        _layoutRepository = layoutRepository;
        _logger = logger;
    }

    public async Task<Page> GetByIdAsync(int id)
    {
        return await _pageRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Page>> GetAllAsync()
    {
        return await _pageRepository.GetAllAsync();
    }

    public async Task<Page> CreatePageAsync(CreatePageDTO dto)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(dto.PageName))
            throw new ValidationException("Page name is required");

        // Layout kontrolü
        var layout = await _layoutRepository.GetByIdAsync(dto.LayoutID);
        if (layout == null)
            throw new ValidationException("Invalid layout");

        var page = new Page
        {
            DepartmentID = dto.DepartmentID,
            PageName = dto.PageName,
            PageTitle = dto.PageTitle,
            LayoutID = dto.LayoutID,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var created = await _pageRepository.AddAsync(page);
        _logger.LogInformation($"Page {dto.PageName} created");

        return created;
    }

    public async Task<PageViewModel> GetPageDetailsAsync(int pageId)
    {
        var page = await GetPageWithLayoutAsync(pageId);
        if (page == null)
            throw new ValidationException("Page not found");

        return new PageViewModel
        {
            PageID = page.PageID,
            PageName = page.PageName,
            PageTitle = page.PageTitle,
            LayoutID = page.LayoutID,
            LayoutName = page.Layout?.LayoutName,
            IsActive = page.IsActive,
            ContentCount = page.Contents?.Count ?? 0,
            CreatedDate = page.CreatedDate
        };
    }

    public async Task<IEnumerable<Page>> GetPagesByDepartmentAsync(int departmentId)
    {
        return await _pageRepository.FindAsync(p =>
            p.DepartmentID == departmentId && p.IsActive
        );
    }

    public async Task<Page> GetPageWithLayoutAsync(int pageId)
    {
        return await _pageRepository.GetWithIncludesAsync(pageId);
    }

    public async Task<Page> UpdateAsync(Page page)
    {
        await _pageRepository.UpdateAsync(page);
        return page;
    }

    public async Task DeleteAsync(int id)
    {
        await _pageRepository.DeleteAsync(id);
        _logger.LogInformation($"Page {id} deleted");
    }
}
```

---

## Layout Service

### ILayoutService Interface

```csharp
public interface ILayoutService : IService<Layout>
{
    Task<Layout> CreateDynamicLayoutAsync(DynamicLayoutDTO dto);
    Task<DynamicLayoutViewModel> GetDynamicLayoutAsync(int layoutId);
    Task<IEnumerable<Layout>> GetCompanyLayoutsAsync(int companyId);
}
```

### LayoutService Implementation

```csharp
public class LayoutService : ILayoutService
{
    private readonly ILayoutRepository _layoutRepository;
    private readonly ILayoutSectionRepository _layoutSectionRepository;
    private readonly ILogger<LayoutService> _logger;

    public async Task<Layout> CreateDynamicLayoutAsync(DynamicLayoutDTO dto)
    {
        // Validation
        if (dto.GridColumnsX < 1 || dto.GridRowsY < 1)
            throw new ValidationException("Grid dimensions must be at least 1");

        if (dto.GridColumnsX > 12 || dto.GridRowsY > 12)
            throw new ValidationException("Grid dimensions cannot exceed 12");

        var layout = new Layout
        {
            CompanyID = dto.CompanyID,
            LayoutName = dto.LayoutName,
            LayoutCode = dto.LayoutCode,
            GridColumnsX = dto.GridColumnsX,
            GridRowsY = dto.GridRowsY,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var created = await _layoutRepository.AddAsync(layout);

        // Create layout sections for each grid position
        var sections = new List<LayoutSection>();
        for (int row = 0; row < dto.GridRowsY; row++)
        {
            for (int col = 0; col < dto.GridColumnsX; col++)
            {
                sections.Add(new LayoutSection
                {
                    LayoutID = created.LayoutID,
                    SectionPosition = $"{(char)('A' + row)}{col + 1}",
                    ColumnIndex = col,
                    RowIndex = row,
                    Width = "100%",
                    Height = "100%",
                    IsActive = true
                });
            }
        }

        await _layoutSectionRepository.AddRangeAsync(sections);
        _logger.LogInformation($"Layout {dto.LayoutName} created with {sections.Count} sections");

        return created;
    }

    public async Task<DynamicLayoutViewModel> GetDynamicLayoutAsync(int layoutId)
    {
        var layout = await _layoutRepository.GetByIdAsync(layoutId);
        if (layout == null)
            throw new ValidationException("Layout not found");

        var sections = await _layoutSectionRepository.GetByLayoutIdAsync(layoutId);

        return new DynamicLayoutViewModel
        {
            LayoutID = layout.LayoutID,
            LayoutName = layout.LayoutName,
            GridColumnsX = layout.GridColumnsX,
            GridRowsY = layout.GridRowsY,
            Sections = sections.Select(s => new DynamicLayoutViewModel.GridSectionDTO
            {
                SectionID = s.LayoutSectionID,
                Position = s.SectionPosition,
                Column = s.ColumnIndex,
                Row = s.RowIndex,
                Width = s.Width,
                Height = s.Height
            }).ToList()
        };
    }

    public async Task<IEnumerable<Layout>> GetCompanyLayoutsAsync(int companyId)
    {
        return await _layoutRepository.FindAsync(l =>
            l.CompanyID == companyId && l.IsActive
        );
    }

    public async Task<Layout> GetByIdAsync(int id)
        => await _layoutRepository.GetByIdAsync(id);

    public async Task<IEnumerable<Layout>> GetAllAsync()
        => await _layoutRepository.GetAllAsync();

    public async Task<Layout> UpdateAsync(Layout layout)
    {
        await _layoutRepository.UpdateAsync(layout);
        return layout;
    }

    public async Task DeleteAsync(int id)
        => await _layoutRepository.DeleteAsync(id);
}
```

---

## Business Logic Patterns

### Validation Pattern

```csharp
public class CreateCompanyValidator : AbstractValidator<CompanyCreateDTO>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required")
            .MaximumLength(255).WithMessage("Company name cannot exceed 255 characters");

        RuleFor(x => x.CompanyCode)
            .NotEmpty().WithMessage("Company code is required")
            .MaximumLength(50).WithMessage("Company code cannot exceed 50 characters")
            .Matches(@"^[A-Z0-9_-]+$").WithMessage("Company code must contain only uppercase letters, numbers, hyphens, and underscores");
    }
}
```

### Exception Handling

```csharp
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class BusinessLogicException : Exception
{
    public BusinessLogicException(string message) : base(message) { }
}
```

### Unit of Work Pattern

```csharp
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ICompanyRepository Companies { get; }
    IDepartmentRepository Departments { get; }
    IPageRepository Pages { get; }

    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public IUserRepository Users { get; }
    public ICompanyRepository Companies { get; }
    public IDepartmentRepository Departments { get; }
    public IPageRepository Pages { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Companies = new CompanyRepository(context);
        Departments = new DepartmentRepository(context);
        Pages = new PageRepository(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
```

---

## Referanslar
- [Service Layer Pattern](https://martinfowler.com/bliki/ServiceLayer.html)
- [Validation Best Practices](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/application-layer-validation)
