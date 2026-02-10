using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DigitalSignage.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        // CRUD Operations
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

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
}
