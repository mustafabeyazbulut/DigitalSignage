using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public interface ILayoutSectionRepository : IRepository<LayoutSection>
    {
        Task<IEnumerable<LayoutSection>> GetSectionsByLayoutAsync(int layoutId);
        Task<LayoutSection?> GetSectionByPositionAsync(int layoutId, int columnIndex, int rowIndex);
        Task<IEnumerable<LayoutSection>> GetActiveSectionsByLayoutAsync(int layoutId);
        Task DeleteSectionsByLayoutAsync(int layoutId);
    }
}
