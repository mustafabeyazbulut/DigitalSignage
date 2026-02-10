using System.Linq.Expressions;
using DigitalSignage.Models.Common;

namespace DigitalSignage.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        // === CRUD Operations ===
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        // === Add / Update / Delete ===
        Task<T> AddAsync(T entity);
        Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteAsync(int id);

        // === Pagination ===
        Task<PagedResult<T>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes);

        // === Counting & Existence ===
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<bool> ExistsAsync(int id);

        // === Include Support (Eager Loading) ===
        Task<IEnumerable<T>> GetAllWithIncludesAsync(params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);
        Task<T?> FirstOrDefaultWithIncludesAsync(
            Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes);

        // === Query Access ===
        IQueryable<T> Query();
        IQueryable<T> QueryAsNoTracking();

        // === SaveChanges ===
        Task<int> SaveChangesAsync();
    }
}
