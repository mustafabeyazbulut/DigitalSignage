using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using DigitalSignage.Data;

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

                // Auto-create LayoutSections based on grid dimensions
                for (int row = 0; row < entity.GridRowsY; row++)
                {
                    for (int col = 0; col < entity.GridColumnsX; col++)
                    {
                        var section = new LayoutSection
                        {
                            LayoutID = entity.LayoutID,
                            SectionPosition = $"{(char)('A' + col)}{row + 1}",
                            ColumnIndex = col,
                            RowIndex = row,
                            Width = "100%",
                            Height = "100%",
                            IsActive = true
                        };
                        await _unitOfWork.LayoutSections.AddAsync(section);
                    }
                }

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
            await _unitOfWork.Layouts.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
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
    }
}
