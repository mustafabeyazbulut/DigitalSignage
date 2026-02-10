using DigitalSignage.Models;

namespace DigitalSignage.Services
{
    public interface IPageService : IService<Page>
    {
        Task<IEnumerable<Page>> GetByDepartmentIdAsync(int departmentId);
    }
}
