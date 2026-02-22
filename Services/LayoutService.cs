using System.Text.Json;
using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using DigitalSignage.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Services
{
    public class LayoutService : ILayoutService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LayoutService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Layout?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Layouts.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Layout>> GetAllAsync()
        {
            return await _unitOfWork.Layouts.GetAllAsync();
        }

        public async Task<Layout> CreateAsync(Layout entity)
        {
            entity.CreatedDate = DateTime.UtcNow;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Layouts.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                // LayoutDefinition JSON'ından otomatik LayoutSection'lar oluştur
                await CreateSectionsFromDefinition(entity.LayoutID, entity.LayoutDefinition);

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            return entity;
        }

        public async Task<Layout> UpdateAsync(Layout entity)
        {
            await _unitOfWork.Layouts.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Bu layout'a ait PageSection'ları temizle
                var sectionIds = await _unitOfWork.LayoutSections
                    .QueryAsNoTracking()
                    .Where(ls => ls.LayoutID == id)
                    .Select(ls => ls.LayoutSectionID)
                    .ToListAsync();

                if (sectionIds.Any())
                {
                    var pageSections = await _unitOfWork.PageSections
                        .Query()
                        .Where(ps => sectionIds.Contains(ps.LayoutSectionID))
                        .ToListAsync();

                    foreach (var ps in pageSections)
                        await _unitOfWork.PageSections.DeleteAsync(ps.PageSectionID);

                    await _unitOfWork.SaveChangesAsync();
                }

                // Bu layout'u kullanan sayfaların LayoutID'sini null yap
                var pages = await _unitOfWork.Pages
                    .Query()
                    .Where(p => p.LayoutID == id)
                    .ToListAsync();

                foreach (var page in pages)
                {
                    page.LayoutID = null;
                    await _unitOfWork.Pages.UpdateAsync(page);
                }
                await _unitOfWork.SaveChangesAsync();

                // Layout'u sil (section'lar cascade ile silinir)
                await _unitOfWork.Layouts.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Layout>> GetByCompanyIdAsync(int companyId)
        {
            return await _unitOfWork.Layouts.GetLayoutsByCompanyAsync(companyId);
        }

        public async Task<Layout?> GetLayoutWithSectionsAsync(int layoutId)
        {
            return await _unitOfWork.Layouts.GetLayoutWithSectionsAsync(layoutId);
        }

        public async Task<IEnumerable<Layout>> GetActiveLayoutsByCompanyAsync(int companyId)
        {
            return await _unitOfWork.Layouts.GetActiveLayoutsByCompanyAsync(companyId);
        }

        public async Task<PagedResult<Layout>> GetLayoutsPagedAsync(
            int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            return await _unitOfWork.Layouts.GetLayoutsPagedAsync(companyId, pageNumber, pageSize, searchTerm, isActive);
        }

        public async Task<bool> IsLayoutInUseAsync(int layoutId)
        {
            return await _unitOfWork.Pages
                .QueryAsNoTracking()
                .AnyAsync(p => p.LayoutID == layoutId);
        }

        public async Task UpdateLayoutDefinitionAsync(int layoutId, string layoutDefinitionJson)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Mevcut bölümleri al
                var existingSections = await _unitOfWork.LayoutSections
                    .Query()
                    .Where(ls => ls.LayoutID == layoutId)
                    .ToListAsync();

                // Önce bu section'lara bağlı PageSection'ları temizle
                var sectionIds = existingSections.Select(s => s.LayoutSectionID).ToList();
                if (sectionIds.Any())
                {
                    var pageSections = await _unitOfWork.PageSections
                        .Query()
                        .Where(ps => sectionIds.Contains(ps.LayoutSectionID))
                        .ToListAsync();

                    foreach (var ps in pageSections)
                        await _unitOfWork.PageSections.DeleteAsync(ps.PageSectionID);

                    await _unitOfWork.SaveChangesAsync();
                }

                // Mevcut bölümleri sil
                foreach (var section in existingSections)
                {
                    await _unitOfWork.LayoutSections.DeleteAsync(section.LayoutSectionID);
                }
                await _unitOfWork.SaveChangesAsync();

                // Düzen tanımını güncelle
                var layout = await _unitOfWork.Layouts.GetByIdAsync(layoutId);
                if (layout != null)
                {
                    layout.LayoutDefinition = layoutDefinitionJson;
                    await _unitOfWork.Layouts.UpdateAsync(layout);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Yeni tanımdan bölümleri yeniden oluştur
                await CreateSectionsFromDefinition(layoutId, layoutDefinitionJson);

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task CreateSectionsFromDefinition(int layoutId, string layoutDefinitionJson)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var definition = JsonSerializer.Deserialize<LayoutDefinitionModel>(layoutDefinitionJson, options);

            if (definition?.Rows == null) return;

            await CreateSectionsRecursive(layoutId, definition.Rows, "");

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Recursive olarak satır/sütun yapısından LayoutSection nesneleri oluşturur.
        /// İç içe sütunlar için pozisyon formatı: R1C1.R1C2 şeklinde nokta ile ayrılır.
        /// </summary>
        private async Task CreateSectionsRecursive(int layoutId, List<LayoutRowDefinition> rows, string parentPrefix)
        {
            for (int rowIdx = 0; rowIdx < rows.Count; rowIdx++)
            {
                var row = rows[rowIdx];
                if (row.Columns == null) continue;

                for (int colIdx = 0; colIdx < row.Columns.Count; colIdx++)
                {
                    var col = row.Columns[colIdx];
                    var position = $"{parentPrefix}R{rowIdx + 1}C{colIdx + 1}";

                    if (col.Rows != null && col.Rows.Count > 0)
                    {
                        // İç içe bölünmüş sütun — alt section'ları recursive oluştur
                        await CreateSectionsRecursive(layoutId, col.Rows, position + ".");
                    }
                    else
                    {
                        // Yaprak (leaf) hücre — section oluştur
                        var section = new LayoutSection
                        {
                            LayoutID = layoutId,
                            SectionPosition = position,
                            ColumnIndex = colIdx,
                            RowIndex = rowIdx,
                            Width = $"{col.Width}%",
                            Height = $"{row.Height}%",
                            IsActive = true
                        };
                        await _unitOfWork.LayoutSections.AddAsync(section);
                    }
                }
            }
        }
    }
}
