using DigitalSignage.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class LayoutSectionRepository : Repository<LayoutSection>, ILayoutSectionRepository
    {
        public LayoutSectionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<LayoutSection>> GetSectionsByLayoutAsync(int layoutId)
        {
            return await _dbSet.AsNoTracking()
                .Where(ls => ls.LayoutID == layoutId)
                .OrderBy(ls => ls.RowIndex)
                .ThenBy(ls => ls.ColumnIndex)
                .ToListAsync();
        }

        public async Task<LayoutSection?> GetSectionByPositionAsync(int layoutId, int columnIndex, int rowIndex)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(ls =>
                    ls.LayoutID == layoutId &&
                    ls.ColumnIndex == columnIndex &&
                    ls.RowIndex == rowIndex);
        }

        public async Task<IEnumerable<LayoutSection>> GetActiveSectionsByLayoutAsync(int layoutId)
        {
            return await _dbSet.AsNoTracking()
                .Where(ls => ls.LayoutID == layoutId && ls.IsActive)
                .OrderBy(ls => ls.RowIndex)
                .ThenBy(ls => ls.ColumnIndex)
                .ToListAsync();
        }

        public async Task DeleteSectionsByLayoutAsync(int layoutId)
        {
            var sections = await _dbSet
                .Where(ls => ls.LayoutID == layoutId)
                .ToListAsync();

            _dbSet.RemoveRange(sections);
        }
    }
}
