using DigitalSignage.Models;

namespace DigitalSignage.Services
{
    public interface IContentService : IService<Content>
    {
        Task<IEnumerable<Content>> GetByDepartmentIdAsync(int departmentId);
    }
}
