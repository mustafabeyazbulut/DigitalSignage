# Deployment & Maintenance

## Genel Bakış

ASP.NET Core 9 MVC uygulamasının kurulum, konfigürasyon, deployment ve bakım prosedürleri.

---

## Development Environment Setup

### Gereklilikler

```
- .NET 9 SDK
- Visual Studio 2022 veya VS Code
- SQL Server 2022+ (Local or Cloud)
- Git
- Node.js (optional, frontend tools için)
```

### SDK Installation

```bash
# .NET 9 SDK indir
https://dotnet.microsoft.com/download/dotnet/9.0

# Kontrol et
dotnet --version
```

### Proje Kurulumu

```bash
# Klonla
git clone https://github.com/your-org/DigitalSignage.git
cd DigitalSignage

# Dependencies yükle
dotnet restore

# Build
dotnet build

# Test
dotnet test
```

---

## Database Setup

### SQL Server Connection String

#### appsettings.json (Development)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DIGITAL_SIGNAGE;Integrated Security=true;Encrypt=false;TrustServerCertificate=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

#### appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql-server.azure.com;Database=DIGITAL_SIGNAGE_PROD;User Id=dbadmin;Password=SecurePassword123!;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

### Database Migrations

```bash
# Yeni migration oluştur
dotnet ef migrations add InitialCreate --context AppDbContext

# Veritabanını güncelle
dotnet ef database update

# Migration'ları listele
dotnet ef migrations list

# Spesifik migration'a geri dön
dotnet ef database update MigrationName
```

### Database Seeding

```csharp
// Program.cs - Startup
var app = builder.Build();

// Database migration
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();

    // Migrate
    context.Database.Migrate();

    // Seed data
    await RoleSeeder.SeedRolesAsync(
        services.GetRequiredService<RoleManager<IdentityRole>>(),
        services.GetRequiredService<UserManager<IdentityUser>>()
    );

    await CompanySeeder.SeedCompaniesAsync(context);
}

app.Run();
```

---

## Configuration

### Environment Variables

```bash
# .env file
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=localhost;Database=DIGITAL_SIGNAGE;...
JWT_SECRET=your-secret-key-here
SMTP_SERVER=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-password
```

### appsettings Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "EmailSettings": {
    "SmtpServer": "",
    "SmtpPort": 587,
    "SenderEmail": "",
    "SenderName": "Digital Signage"
  },
  "JwtSettings": {
    "Secret": "",
    "Issuer": "DigitalSignage",
    "Audience": "SignageUsers",
    "ExpirationMinutes": 60
  },
  "AppSettings": {
    "MaxUploadSize": 104857600,
    "AllowedFileTypes": ["jpg", "png", "mp4", "pdf"],
    "EnableScheduledJobs": true,
    "ScheduleCheckIntervalMinutes": 5
  }
}
```

---

## Local Development

### Running the Application

```bash
# Debug mode
dotnet run --configuration Debug

# Release mode
dotnet run --configuration Release

# Port belirt
dotnet run --urls "http://localhost:5000"
```

### Visual Studio Debugging

```
- F5: Debug start
- Ctrl+F5: Run without debugging
- Breakpoints koy
- Watch/Locals panelleri kullan
```

---

## Build & Publish

### Build Process

```bash
# Debug Build
dotnet build

# Release Build
dotnet build -c Release

# Self-contained executable (EXE)
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained

# Framework-dependent (requires .NET runtime)
dotnet publish -c Release
```

### Publish Output

```
bin/Release/net9.0/publish/
├── DigitalSignage.exe
├── DigitalSignage.dll
├── appsettings.json
├── web.config (IIS için)
└── ...
```

---

## Hosting Options

### Windows Server + IIS

#### 1. IIS'de Application Pool Oluştur

```
- Pool Name: SignagePool
- .NET CLR Version: No Managed Code
- Pipeline Mode: Integrated
```

#### 2. Web Site Oluştur

```
- Site Name: DigitalSignage
- Physical Path: C:\inetpub\DigitalSignage
- Port: 80 (HTTP) veya 443 (HTTPS)
```

#### 3. web.config

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <aspNetCore
      processPath="dotnet"
      arguments=".\DigitalSignage.dll"
      stdoutLogEnabled="true"
      stdoutLogFile=".\logs\stdout"
      hostingModel="inprocess" />
  </system.webServer>
</configuration>
```

#### 4. Deploy

```bash
# Publish
dotnet publish -c Release

# Copy to IIS folder
xcopy bin\Release\net9.0\publish\* "C:\inetpub\DigitalSignage" /E /Y
```

### Docker

#### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj
COPY ["DigitalSignage/DigitalSignage.csproj", "DigitalSignage/"]
RUN dotnet restore "DigitalSignage/DigitalSignage.csproj"

# Copy source
COPY . .
WORKDIR "/src/DigitalSignage"
RUN dotnet build "DigitalSignage.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "DigitalSignage.csproj" -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "DigitalSignage.dll"]
```

#### docker-compose.yml

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "YourPassword123!"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  app:
    build:
      context: .
      dockerfile: Dockerfile
    depends_on:
      - sqlserver
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Server=sqlserver;User Id=sa;Password=YourPassword123!;Database=DIGITAL_SIGNAGE;"
    ports:
      - "5000:80"
    volumes:
      - app_logs:/app/logs

volumes:
  sqlserver_data:
  app_logs:
```

#### Docker Commands

```bash
# Build
docker build -t signage:latest .

# Run
docker run -d \
  --name signage-app \
  -p 5000:80 \
  -e ConnectionStrings__DefaultConnection="..." \
  signage:latest

# Docker Compose
docker-compose up -d
docker-compose down
```

### Azure App Service

```bash
# Resource Group oluştur
az group create --name SignageRG --location eastus

# App Service Plan oluştur
az appservice plan create \
  --name SignagePlan \
  --resource-group SignageRG \
  --sku B2 \
  --is-linux

# Web App oluştur
az webapp create \
  --resource-group SignageRG \
  --plan SignagePlan \
  --name digital-signage

# Publish
dotnet publish -c Release
az webapp up --resource-group SignageRG --name digital-signage
```

---

## HTTPS & SSL/TLS

### Self-Signed Certificate (Development)

```bash
# Sertifika oluştur
dotnet dev-certs https --trust

# Sertifikaları listele
dotnet dev-certs https --list
```

### Production SSL

```bash
# Obtain certificate from Let's Encrypt (IIS)
# Using certbot or Azure Key Vault

# Update appsettings
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://+:443",
        "Certificate": {
          "Path": "/etc/ssl/certs/certificate.pfx",
          "Password": "cert-password"
        }
      }
    }
  }
}
```

---

## Monitoring & Logging

### Application Insights (Azure)

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(
    builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
);

// appsettings.json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-key-here"
  }
}
```

### File Logging

```csharp
// appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    },
    "File": {
      "Path": "logs/app-{Date:yyyy-MM-dd}.log",
      "IncludeScopes": true
    }
  }
}

// Program.cs
var logPath = builder.Configuration["Logging:File:Path"];
builder.Logging.AddFile(logPath);
```

### Health Checks

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddUrlGroup(new Uri("https://example.com"));

app.MapHealthChecks("/health");
```

---

## Performance Optimization

### Caching

```csharp
// Distributed Cache Setup
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

// Usage
private readonly IDistributedCache _cache;

public async Task<Company> GetCompanyAsync(int id)
{
    var key = $"company_{id}";
    var cached = await _cache.GetStringAsync(key);

    if (cached != null)
        return JsonSerializer.Deserialize<Company>(cached);

    var company = await _repository.GetByIdAsync(id);
    await _cache.SetStringAsync(key, JsonSerializer.Serialize(company),
        new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        }
    );

    return company;
}
```

### Query Optimization

```csharp
// Use Include to prevent N+1
var companies = await _context.Companies
    .Include(c => c.SystemUnits)
    .Include(c => c.UserCompanyRoles)
    .AsNoTracking()  // Read-only queries için
    .ToListAsync();

// Use Select for projection
var companyDtos = await _context.Companies
    .Select(c => new CompanyDTO
    {
        CompanyID = c.CompanyID,
        CompanyName = c.CompanyName,
        SystemCount = c.SystemUnits.Count()
    })
    .ToListAsync();
```

### Compression

```csharp
// Program.cs
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.Providers.Add<BrotliCompressionProvider>();
});

app.UseResponseCompression();
```

---

## Backup & Disaster Recovery

### Database Backup

```bash
# SQL Server Backup
BACKUP DATABASE [DIGITAL_SIGNAGE]
TO DISK = 'C:\Backups\DIGITAL_SIGNAGE_Full.bak'

# Restore
RESTORE DATABASE [DIGITAL_SIGNAGE]
FROM DISK = 'C:\Backups\DIGITAL_SIGNAGE_Full.bak'
```

### Azure Backup

```bash
# Automatic backup setup
az backup protection enable-for-vm \
  --resource-group SignageRG \
  --vault-name SignageBackupVault \
  --vm signage-vm
```

---

## Update & Maintenance

### Update Procedures

```bash
# Stash changes
git stash

# Pull latest
git pull origin main

# Install new dependencies
dotnet restore

# Run migrations
dotnet ef database update

# Restart application
# (IIS: recycle app pool, Docker: restart container)
```

### Rolling Updates (Zero-Downtime)

```bash
# Two instances running
# Stop instance 1
# Update instance 1
# Start instance 1
# Stop instance 2
# Update instance 2
# Start instance 2
```

---

## Monitoring Dashboard

### Key Metrics

```
- API Response Time: < 200ms
- Database Query Time: < 50ms
- Error Rate: < 0.1%
- Uptime: > 99.9%
- User Sessions: Active count
- Page Load Time: < 2s
```

### Alerts

```csharp
// Setup monitoring
- High error rate alert
- Database connection failure
- Disk space critical
- Memory usage > 80%
- Response time > 1s
```

---

## Security Checklist

- [ ] HTTPS enabled
- [ ] SQL injection prevention
- [ ] XSS protection enabled
- [ ] CSRF tokens in forms
- [ ] Password hashing (bcrypt)
- [ ] Input validation
- [ ] Rate limiting
- [ ] Security headers
- [ ] CORS configured
- [ ] Regular backups
- [ ] Dependency updates
- [ ] Security scanning

---

## Troubleshooting

### Common Issues

#### 1. Connection String Error
```
Fix: Check appsettings.json, verify SQL Server is running
```

#### 2. Migration Failed
```bash
# Rollback
dotnet ef database update PreviousMigrationName

# Drop and recreate
dotnet ef database drop
dotnet ef database update
```

#### 3. Port Already in Use
```bash
# Find process
netstat -ano | findstr :5000

# Kill process
taskkill /PID <PID> /F
```

#### 4. High Memory Usage
```csharp
// Enable garbage collection
builder.Services.AddResponseCaching();

// Limit connection pool
optionsBuilder.UseSqlServer(connectionString,
    options => options.MaxPoolSize = 20);
```

---

## API Documentation

### Swagger/OpenAPI

```csharp
// Program.cs
builder.Services.AddSwaggerGen();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Signage API V1");
    c.RoutePrefix = string.Empty;
});
```

### Access Swagger
```
http://localhost:5000/swagger
```

---

## Referanslar
- [ASP.NET Core Deployment](https://docs.microsoft.com/aspnet/core/host-and-deploy)
- [Azure App Service](https://docs.microsoft.com/azure/app-service/)
- [Docker for .NET](https://docs.microsoft.com/dotnet/architecture/containerized-lifecycle)
- [Entity Framework Core Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations)
