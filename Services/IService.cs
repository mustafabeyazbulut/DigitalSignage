using System.Collections.Generic;
using System.Threading.Tasks;

namespace DigitalSignage.Services
{
    public interface IService<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}
