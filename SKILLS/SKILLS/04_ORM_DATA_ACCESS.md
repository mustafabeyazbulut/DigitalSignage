# ORM & Data Access Layer (Entity Framework Core)

## Genel Bakış

Entity Framework Core 9, ASP.NET Core 9 MVC uygulamasının veri erişim katmanını sağlar. Veritabanına object-oriented şekilde erişim imkanı verir.

---

## DbContext Yapısı

### AppDbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // DbSet'ler - Entity Collections
    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<SystemUnit> SystemUnits { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Layout> Layouts { get; set; }
    public DbSet<LayoutSection> LayoutSections { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<PageSection> PageSections { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<SchedulePage> SchedulePages { get; set; }
    public DbSet<UserCompanyRole> UserCompanyRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Entity Configurations
        ConfigureUserEntity(modelBuilder);
        ConfigureCompanyEntity(modelBuilder);
        ConfigureSystemUnitEntity(modelBuilder);
        ConfigureDepartmentEntity(modelBuilder);
        ConfigureLayoutEntity(modelBuilder);
        ConfigurePageEntity(modelBuilder);
        ConfigureScheduleEntity(modelBuilder);
        ConfigureRelationships(modelBuilder);
    }

    // Configuration Methods
    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserID);
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    private void ConfigureCompanyEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyID);
            entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CompanyCode).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.CompanyCode).IsUnique();
        });
    }

    private void ConfigureSystemUnitEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemUnit>(entity =>
        {
            entity.HasKey(e => e.SystemUnitID);
            entity.Property(e => e.SystemName).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => new { e.CompanyID, e.SystemCode }).IsUnique();
        });
    }

    private void ConfigureDepartmentEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentID);
            entity.Property(e => e.DepartmentName).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => new { e.SystemUnitID, e.DepartmentCode }).IsUnique();
        });
    }

    private void ConfigureLayoutEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Layout>(entity =>
        {
            entity.HasKey(e => e.LayoutID);
            entity.Property(e => e.LayoutName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.GridColumnsX).HasDefaultValue(1);
            entity.Property(e => e.GridRowsY).HasDefaultValue(1);
        });
    }

    private void ConfigurePageEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Page>(entity =>
        {
            entity.HasKey(e => e.PageID);
            entity.Property(e => e.PageName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PageTitle).IsRequired().HasMaxLength(255);
        });
    }

    private void ConfigureScheduleEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleID);
            entity.Property(e => e.ScheduleName).IsRequired().HasMaxLength(255);
        });
    }

    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // Company → SystemUnit (1:N)
        modelBuilder.Entity<SystemUnit>()
            .HasOne(s => s.Company)
            .WithMany(c => c.SystemUnits)
            .HasForeignKey(s => s.CompanyID)
            .OnDelete(DeleteBehavior.Cascade);

        // Company → Department (1:N)
        modelBuilder.Entity<Department>()
            .HasOne(d => d.Company)
            .WithMany(c => c.Departments)
            .HasForeignKey(d => d.CompanyID)
            .OnDelete(DeleteBehavior.Cascade);

        // SystemUnit → Department (1:N)
        modelBuilder.Entity<Department>()
            .HasOne(d => d.SystemUnit)
            .WithMany(s => s.Departments)
            .HasForeignKey(d => d.SystemUnitID)
            .OnDelete(DeleteBehavior.Cascade);

        // Department → Page (1:N)
        modelBuilder.Entity<Page>()
            .HasOne(p => p.Department)
            .WithMany(d => d.Pages)
            .HasForeignKey(p => p.DepartmentID)
            .OnDelete(DeleteBehavior.Cascade);

        // Layout → LayoutSection (1:N)
        modelBuilder.Entity<LayoutSection>()
            .HasOne(ls => ls.Layout)
            .WithMany(l => l.LayoutSections)
            .HasForeignKey(ls => ls.LayoutID)
            .OnDelete(DeleteBehavior.Cascade);

        // Page → Content (1:N)
        modelBuilder.Entity<Content>()
            .HasOne(c => c.Page)
            .WithMany(p => p.Contents)
            .HasForeignKey(c => c.PageID)
            .OnDelete(DeleteBehavior.Cascade);

        // Schedule → SchedulePage (1:N)
        modelBuilder.Entity<SchedulePage>()
            .HasOne(sp => sp.Schedule)
            .WithMany(s => s.SchedulePages)
            .HasForeignKey(sp => sp.ScheduleID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## Dependency Injection Configuration

Program.cs dosyasında DbContext ve Repository'ler kaydedilir:

```csharp
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    // Add DbContext
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    );

    // Add Repositories (Generic Repository Pattern)
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

    // Add Specific Repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
    builder.Services.AddScoped<ISystemUnitRepository, SystemUnitRepository>();
    builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
    builder.Services.AddScoped<IPageRepository, PageRepository>();
    builder.Services.AddScoped<ILayoutRepository, LayoutRepository>();
    builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();

    // Add Services
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ICompanyService, CompanyService>();
    builder.Services.AddScoped<ISystemService, SystemService>();
    builder.Services.AddScoped<IDepartmentService, DepartmentService>();

    // Add MVC
    builder.Services.AddControllersWithViews();

    var app = builder.Build();
    app.MapControllers();
    app.Run();
}
```

---

## Repository Pattern

### Generic Repository Interface

```csharp
public interface IRepository<T> where T : class
{
    // CRUD Operations
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    // Add/Update/Delete
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteAsync(int id);

    // SaveChanges
    Task<int> SaveChangesAsync();

    // IQueryable for Complex Queries
    IQueryable<T> Query();
}
```

### Generic Repository Implementation

```csharp
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await SaveChangesAsync();
        return entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        await SaveChangesAsync();
        return entities;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await SaveChangesAsync();
        }
    }

    public virtual async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }
}
```

### Specialized Repositories

```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User> GetByUserNameAsync(string userName);
    Task<User> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User> GetByUserNameAsync(string userName)
    {
        return await FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await FindAsync(u => u.IsActive);
    }
}
```

---

## Entity Framework Core Migrations

### Yeni Migration Oluşturma

```bash
# Migration oluştur
dotnet ef migrations add AddCompanySystem --context AppDbContext

# Veritabanına uygula
dotnet ef database update
```

### Migration Dosyası Örneği

```csharp
public partial class AddCompanySystem : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Companies",
            columns: table => new
            {
                CompanyID = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CompanyName = table.Column<string>(maxLength: 255, nullable: false),
                CompanyCode = table.Column<string>(maxLength: 50, nullable: false),
                Description = table.Column<string>(maxLength: 500, nullable: true),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                ModifiedDate = table.Column<DateTime>(nullable: true),
                ModifiedBy = table.Column<string>(maxLength: 255, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Companies", x => x.CompanyID);
                table.UniqueConstraint("UQ_Companies_Code", x => x.CompanyCode);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Companies_CompanyCode",
            table: "Companies",
            column: "CompanyCode");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Companies");
    }
}
```

---

## LINQ Sorguları Örnekleri

### Basit Queries

```csharp
// Tüm şirketleri getir
var allCompanies = await _context.Companies.ToListAsync();

// ID'ye göre şirket getir
var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyID == id);

// Aktif şirketleri getir
var activeCompanies = await _context.Companies
    .Where(c => c.IsActive)
    .ToListAsync();
```

### Join Sorguları

```csharp
// Şirket ile departmanlarını getir (Include)
var company = await _context.Companies
    .Include(c => c.Departments)
    .FirstOrDefaultAsync(c => c.CompanyID == id);

// Çoklu Include
var department = await _context.Departments
    .Include(d => d.Company)
    .Include(d => d.SystemUnit)
    .Include(d => d.Pages)
    .FirstOrDefaultAsync(d => d.DepartmentID == id);

// Select Projection
var companyDtos = await _context.Companies
    .Select(c => new CompanyViewModel
    {
        CompanyID = c.CompanyID,
        CompanyName = c.CompanyName,
        DepartmentCount = c.Departments.Count(),
        SystemCount = c.SystemUnits.Count()
    })
    .ToListAsync();
```

### Filtering ve Sorting

```csharp
// Filtreleme ve Sıralama
var pages = await _context.Pages
    .Where(p => p.DepartmentID == departmentId && p.IsActive)
    .OrderBy(p => p.PageName)
    .ToListAsync();

// Pagination
var pageSize = 10;
var pageNumber = 1;
var pages = await _context.Pages
    .Where(p => p.DepartmentID == departmentId)
    .OrderBy(p => p.PageName)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### Aggregation

```csharp
// Count
var departmentCount = await _context.Departments
    .Where(d => d.CompanyID == companyId)
    .CountAsync();

// Sum, Average
var totalLayouts = await _context.Layouts
    .Where(l => l.CompanyID == companyId)
    .SumAsync(l => l.GridColumnsX * l.GridRowsY);
```

### Complex Queries

```csharp
// Şirketin tüm sayfalarını al (nested navigations)
var pages = await _context.Pages
    .Where(p => p.Department.Company.CompanyID == companyId)
    .Include(p => p.Department)
    .Include(p => p.Layout)
        .ThenInclude(l => l.LayoutSections)
    .Include(p => p.Contents)
    .ToListAsync();
```

---

## Change Tracking

```csharp
// Manual Change Tracking
var user = await _context.Users.FindAsync(id);
user.LastName = "NewLastName";

// EF Core otomatik olarak UpdateAsync yapmadan yapılırsa:
_context.Users.Update(user);
await _context.SaveChangesAsync();

// Veyaexplicit
_context.Entry(user).State = EntityState.Modified;
await _context.SaveChangesAsync();
```

---

## Concurrency Handling

```csharp
// Optimistic Concurrency
public class Company
{
    // ...
    [Timestamp]
    public byte[] RowVersion { get; set; }
}

// Update sırasında RowVersion kontrol
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Conflict handling
}
```

---

## Referanslar
- [EF Core Documentation](https://docs.microsoft.com/ef/core/)
- [LINQ to Entities](https://docs.microsoft.com/dotnet/csharp/programming-guide/concepts/linq/)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
