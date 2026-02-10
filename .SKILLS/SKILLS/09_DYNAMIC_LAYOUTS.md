# Dinamik Sayfa Tasarımları (X-Y Grid System)

## Genel Bakış

Dijital işaretleme ekranlarında gösterilecek sayfaların düzeni dinamik olarak X (yatay) ve Y (dikey) eksenlerinde bölünebilir. Bu sistem responsive ve tamamen konfigüre edilebilirdir.

---

## Grid Konsepti

### X-Y Grid Tanımı

```
X = Yatay bölüm sayısı (Sütun)
Y = Dikey bölüm sayısı (Satır)

Örnek: 2x2 Grid (4 bölüm)
┌─────────────┬─────────────┐
│   A1        │   A2        │  Y
├─────────────┼─────────────┤
│   B1        │   B2        │
└─────────────┴─────────────┘
      X

Örnek: 3x3 Grid (9 bölüm)
┌──────┬──────┬──────┐
│ A1   │ A2   │ A3   │
├──────┼──────┼──────┤
│ B1   │ B2   │ B3   │
├──────┼──────┼──────┤
│ C1   │ C2   │ C3   │
└──────┴──────┴──────┘
```

### Desteklenen Grid Boyutları

| Grid | Total Sections | Kullanım Alanı |
|------|---|---|
| 1x1 | 1 | Full screen |
| 2x1 | 2 | 2 yan yana |
| 1x2 | 2 | 2 üst/alt |
| 2x2 | 4 | Quad layout |
| 3x2 | 6 | Kompleks layout |
| 2x3 | 6 | Dikey layout |
| 3x3 | 9 | Maksimum bölüm |
| 4x3 | 12 | Ultra yoğun |

---

## Layout Entity

### Layout Table Structure

```csharp
public class Layout
{
    public int LayoutID { get; set; }
    public int CompanyID { get; set; }

    // Grid Configuration
    public int GridColumnsX { get; set; }  // 1-12
    public int GridRowsY { get; set; }      // 1-12

    // Metadata
    public string LayoutName { get; set; }
    public string LayoutCode { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation
    public Company Company { get; set; }
    public ICollection<LayoutSection> LayoutSections { get; set; }
    public ICollection<Page> Pages { get; set; }
}
```

### Layout Validation Rules

```csharp
public class LayoutValidator : AbstractValidator<Layout>
{
    public LayoutValidator()
    {
        RuleFor(x => x.GridColumnsX)
            .GreaterThanOrEqualTo(1).WithMessage("Minimum 1 column required")
            .LessThanOrEqualTo(12).WithMessage("Maximum 12 columns allowed");

        RuleFor(x => x.GridRowsY)
            .GreaterThanOrEqualTo(1).WithMessage("Minimum 1 row required")
            .LessThanOrEqualTo(12).WithMessage("Maximum 12 rows allowed");

        RuleFor(x => x.LayoutName)
            .NotEmpty().WithMessage("Layout name is required")
            .MaximumLength(255).WithMessage("Layout name cannot exceed 255 characters");
    }
}
```

---

## LayoutSection Entity

### Section Tanımı

Her bölüm grid içinde belirli bir koordinatı temsil eder:

```csharp
public class LayoutSection
{
    public int LayoutSectionID { get; set; }
    public int LayoutID { get; set; }

    // Position Information
    public string SectionPosition { get; set; }  // "A1", "B2", vb.
    public int ColumnIndex { get; set; }         // 0-11
    public int RowIndex { get; set; }            // 0-11

    // Styling
    public string Width { get; set; }   // "100%", "50%", vb.
    public string Height { get; set; }  // "100%", "50%", vb.

    // Status
    public bool IsActive { get; set; }

    // Navigation
    public Layout Layout { get; set; }
    public ICollection<PageSection> PageSections { get; set; }
}
```

### Section Position Naming

```
2x2 Grid:
Row 0: A1  A2
Row 1: B1  B2

3x3 Grid:
Row 0: A1  A2  A3
Row 1: B1  B2  B3
Row 2: C1  C2  C3

Position Formula:
Letter = (char)('A' + RowIndex)
Number = ColumnIndex + 1
Position = $"{Letter}{Number}"
```

---

## Dynamic Layout Service

### ILayoutService Interface

```csharp
public interface ILayoutService : IService<Layout>
{
    // Create & Manage
    Task<Layout> CreateDynamicLayoutAsync(DynamicLayoutDTO dto);
    Task<Layout> UpdateGridDimensionsAsync(int layoutId, int columnsX, int rowsY);

    // Retrieve
    Task<DynamicLayoutViewModel> GetDynamicLayoutAsync(int layoutId);
    Task<IEnumerable<Layout>> GetCompanyLayoutsAsync(int companyId);

    // Sections
    Task<IEnumerable<LayoutSection>> GetLayoutSectionsAsync(int layoutId);
    Task<LayoutSection> GetSectionAsync(int sectionId);
    Task<LayoutSection> GetSectionByPositionAsync(int layoutId, string position);

    // Validation
    Task<bool> ValidateGridDimensionsAsync(int columnsX, int rowsY);
}
```

### LayoutService Implementation

```csharp
public class LayoutService : ILayoutService
{
    private readonly ILayoutRepository _layoutRepository;
    private readonly ILayoutSectionRepository _layoutSectionRepository;
    private readonly IPageRepository _pageRepository;
    private readonly ILogger<LayoutService> _logger;

    public async Task<Layout> CreateDynamicLayoutAsync(DynamicLayoutDTO dto)
    {
        // Validation
        if (!await ValidateGridDimensionsAsync(dto.GridColumnsX, dto.GridRowsY))
            throw new ValidationException("Invalid grid dimensions");

        var layout = new Layout
        {
            CompanyID = dto.CompanyID,
            LayoutName = dto.LayoutName,
            LayoutCode = dto.LayoutCode,
            GridColumnsX = dto.GridColumnsX,
            GridRowsY = dto.GridRowsY,
            Description = dto.Description,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        // Save layout
        var createdLayout = await _layoutRepository.AddAsync(layout);

        // Create sections automatically
        await GenerateLayoutSectionsAsync(createdLayout.LayoutID, dto.GridColumnsX, dto.GridRowsY);

        _logger.LogInformation(
            $"Layout {dto.LayoutName} ({dto.GridColumnsX}x{dto.GridRowsY}) created with ID {createdLayout.LayoutID}"
        );

        return createdLayout;
    }

    public async Task<Layout> UpdateGridDimensionsAsync(int layoutId, int columnsX, int rowsY)
    {
        // Validation
        if (!await ValidateGridDimensionsAsync(columnsX, rowsY))
            throw new ValidationException("Invalid grid dimensions");

        var layout = await _layoutRepository.GetByIdAsync(layoutId);
        if (layout == null)
            throw new ValidationException("Layout not found");

        // Eğer grid boyutu değişirse, bölümleri yeniden oluştur
        if (layout.GridColumnsX != columnsX || layout.GridRowsY != rowsY)
        {
            // Eski bölümleri sil
            var oldSections = await _layoutSectionRepository.FindAsync(ls => ls.LayoutID == layoutId);
            foreach (var section in oldSections)
            {
                await _layoutSectionRepository.DeleteAsync(section);
            }

            // Yeni grid boyutlarını ayarla
            layout.GridColumnsX = columnsX;
            layout.GridRowsY = rowsY;

            // Yeni bölümleri oluştur
            await GenerateLayoutSectionsAsync(layoutId, columnsX, rowsY);

            _logger.LogInformation($"Layout {layoutId} grid resized to {columnsX}x{rowsY}");
        }

        await _layoutRepository.UpdateAsync(layout);
        return layout;
    }

    public async Task<DynamicLayoutViewModel> GetDynamicLayoutAsync(int layoutId)
    {
        var layout = await _layoutRepository.GetByIdAsync(layoutId);
        if (layout == null)
            throw new ValidationException("Layout not found");

        var sections = await _layoutSectionRepository.FindAsync(ls => ls.LayoutID == layoutId);

        return new DynamicLayoutViewModel
        {
            LayoutID = layout.LayoutID,
            CompanyID = layout.CompanyID,
            LayoutName = layout.LayoutName,
            GridColumnsX = layout.GridColumnsX,
            GridRowsY = layout.GridRowsY,
            Sections = sections
                .OrderBy(s => s.RowIndex)
                .ThenBy(s => s.ColumnIndex)
                .Select(s => new DynamicLayoutViewModel.GridSectionDTO
                {
                    SectionID = s.LayoutSectionID,
                    Position = s.SectionPosition,
                    Column = s.ColumnIndex,
                    Row = s.RowIndex,
                    Width = s.Width,
                    Height = s.Height
                })
                .ToList()
        };
    }

    public async Task<bool> ValidateGridDimensionsAsync(int columnsX, int rowsY)
    {
        return columnsX >= 1 && columnsX <= 12 &&
               rowsY >= 1 && rowsY <= 12;
    }

    private async Task GenerateLayoutSectionsAsync(int layoutId, int columnsX, int rowsY)
    {
        var sections = new List<LayoutSection>();

        for (int row = 0; row < rowsY; row++)
        {
            for (int col = 0; col < columnsX; col++)
            {
                sections.Add(new LayoutSection
                {
                    LayoutID = layoutId,
                    SectionPosition = GenerateSectionPosition(row, col),
                    ColumnIndex = col,
                    RowIndex = row,
                    Width = "100%",
                    Height = "100%",
                    IsActive = true
                });
            }
        }

        await _layoutSectionRepository.AddRangeAsync(sections);
    }

    private string GenerateSectionPosition(int rowIndex, int columnIndex)
    {
        var letter = (char)('A' + rowIndex);
        var number = columnIndex + 1;
        return $"{letter}{number}";
    }

    // Diğer metodlar...
}
```

---

## Frontend - Grid Rendering

### CSS Grid Layout

```html
<div class="layout-grid"
     style="display: grid;
             grid-template-columns: repeat(@Model.GridColumnsX, 1fr);
             grid-template-rows: repeat(@Model.GridRowsY, auto);
             gap: 10px;
             width: 100%;
             height: 100vh;">

    @foreach (var section in Model.Sections)
    {
        <div class="section @section.Position"
             style="grid-column: @(section.Column + 1);
                     grid-row: @(section.Row + 1);
                     background: #f0f0f0;
                     border: 2px solid #ddd;
                     padding: 15px;
                     overflow: hidden;">

            <h5>@section.Position</h5>
            <div id="content-@section.SectionID" class="section-content">
                <!-- İçerik buraya yüklenecek -->
            </div>
        </div>
    }
</div>
```

### JavaScript - Dynamic Content Loading

```javascript
// Grid initialize
async function initializeGrid(layoutId) {
    try {
        const response = await fetch(`/api/layout/${layoutId}`);
        const layout = await response.json();

        // Grid oluştur
        createGridLayout(layout);

        // Her bölüme içerik yükle
        for (const section of layout.sections) {
            await loadSectionContent(section.sectionId, section.position);
        }
    } catch (error) {
        console.error('Error loading layout:', error);
    }
}

function createGridLayout(layout) {
    const grid = document.querySelector('.layout-grid');
    grid.style.gridTemplateColumns = `repeat(${layout.gridColumnsX}, 1fr)`;
    grid.style.gridTemplateRows = `repeat(${layout.gridRowsY}, 1fr)`;
}

async function loadSectionContent(sectionId, position) {
    try {
        const response = await fetch(`/api/section/${sectionId}/content`);
        const content = await response.json();

        const contentDiv = document.querySelector(`#content-${sectionId}`);
        if (contentDiv) {
            contentDiv.innerHTML = content.html;
        }
    } catch (error) {
        console.error(`Error loading section ${position}:`, error);
    }
}

// Responsive adjustment
window.addEventListener('resize', adjustGridSize);

function adjustGridSize() {
    const container = document.querySelector('.layout-grid');
    const width = container.offsetWidth;
    const height = container.offsetHeight;

    // Dynamic sizing logic
    document.querySelectorAll('.section').forEach(section => {
        // Adaptive sizing
    });
}
```

---

## Grid Configuration Examples

### 2x2 Grid (Quad Layout)

```csharp
// 4 ayrı kamera feed
{
    "layoutName": "4-Camera Monitor",
    "gridColumnsX": 2,
    "gridRowsY": 2,
    "sections": [
        {"position": "A1", "content": "Camera 1"},
        {"position": "A2", "content": "Camera 2"},
        {"position": "B1", "content": "Camera 3"},
        {"position": "B2", "content": "Camera 4"}
    ]
}
```

### 3x2 Grid (Yaygın layout)

```csharp
// Banner + 5 content area
{
    "layoutName": "Dashboard Layout",
    "gridColumnsX": 3,
    "gridRowsY": 2,
    "sections": [
        {"position": "A1", "span": "3x1", "content": "Header Banner"},
        {"position": "B1", "content": "News 1"},
        {"position": "B2", "content": "News 2"},
        {"position": "B3", "content": "News 3"}
    ]
}
```

### 1x1 Grid (Full Screen)

```csharp
// Tek bir full-screen gösterim
{
    "layoutName": "Full Screen",
    "gridColumnsX": 1,
    "gridRowsY": 1,
    "sections": [
        {"position": "A1", "content": "Full screen video"}
    ]
}
```

---

## Page Layout Assignment

### PageSection (Sayfa & Layout İlişkisi)

```csharp
public class PageSection
{
    public int PageSectionID { get; set; }
    public int PageID { get; set; }
    public int LayoutSectionID { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    // Navigation
    public Page Page { get; set; }
    public LayoutSection LayoutSection { get; set; }
}
```

### Sayfa-Layout Bind Etme

```csharp
// Sayfaya layout ata
public async Task<Page> AssignLayoutToPageAsync(int pageId, int layoutId)
{
    var page = await _pageRepository.GetByIdAsync(pageId);
    var layout = await _layoutRepository.GetByIdAsync(layoutId);

    if (page == null || layout == null)
        throw new ValidationException("Page or Layout not found");

    page.LayoutID = layoutId;
    await _pageRepository.UpdateAsync(page);

    // Layout sections'ları page sections'a dönüştür
    var layoutSections = await _layoutSectionRepository.FindAsync(
        ls => ls.LayoutID == layoutId && ls.IsActive
    );

    foreach (var layoutSection in layoutSections)
    {
        var pageSection = new PageSection
        {
            PageID = pageId,
            LayoutSectionID = layoutSection.LayoutSectionID,
            DisplayOrder = layoutSection.LayoutSectionID,
            IsActive = true
        };

        await _pageSectionRepository.AddAsync(pageSection);
    }

    return page;
}
```

---

## Layout Controller

```csharp
[Route("api/companies/{companyId}/layouts")]
[ApiController]
[Authorize]
public class LayoutController : BaseController
{
    private readonly ILayoutService _layoutService;
    private readonly ITenantContext _tenantContext;
    private readonly IMapper _mapper;

    /// <summary>
    /// Dinamik layout oluştur
    /// </summary>
    [HttpPost("create")]
    [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> CreateDynamicLayout(
        int companyId,
        [FromBody] DynamicLayoutDTO dto)
    {
        try
        {
            if (!await _tenantContext.IsCompanyAdminAsync(companyId))
                return Forbid();

            var layout = await _layoutService.CreateDynamicLayoutAsync(dto);
            var viewModel = _mapper.Map<DynamicLayoutViewModel>(layout);

            return CreatedAtAction(
                nameof(GetDynamicLayout),
                new { companyId, layoutId = layout.LayoutID },
                viewModel
            );
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Grid boyutlarını güncelle
    /// </summary>
    [HttpPut("{layoutId}/resize")]
    [Authorize(Roles = "CompanyAdmin")]
    public async Task<IActionResult> ResizeGrid(
        int companyId,
        int layoutId,
        [FromBody] ResizeGridRequest request)
    {
        try
        {
            if (!await _tenantContext.IsCompanyAdminAsync(companyId))
                return Forbid();

            var layout = await _layoutService.UpdateGridDimensionsAsync(
                layoutId,
                request.GridColumnsX,
                request.GridRowsY
            );

            return Ok(layout);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Layout'u getir
    /// </summary>
    [HttpGet("{layoutId}")]
    public async Task<IActionResult> GetDynamicLayout(int companyId, int layoutId)
    {
        try
        {
            if (!await _tenantContext.HasAccessToCompanyAsync(companyId))
                return Forbid();

            var layout = await _layoutService.GetDynamicLayoutAsync(layoutId);
            return Ok(layout);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
```

---

## Best Practices

### 1. Grid Optimization
- Maksimum 12x12 grid (144 bölüm)
- Responsive design kullan
- Bölümleri lazy-load et

### 2. Performance
```csharp
// Include'larla optimize et
var layout = await _layoutRepository
    .Query()
    .Include(l => l.LayoutSections)
    .FirstOrDefaultAsync(l => l.LayoutID == id);
```

### 3. Validation
```csharp
// Grid boyutlarını doğrula
if (gridX < 1 || gridX > 12 || gridY < 1 || gridY > 12)
    throw new ValidationException("Invalid grid dimensions");
```

---

## Referanslar
- [CSS Grid Layout](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Grid_Layout)
- [Responsive Design](https://www.w3schools.com/css/css_rwd_intro.asp)
