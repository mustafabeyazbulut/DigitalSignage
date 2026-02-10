using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Department>> GetDepartmentsByCompanyAsync(int companyId)
        {
            return await _dbSet.AsNoTracking()
                .Where(d => d.CompanyID == companyId)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        public async Task<Department?> GetDepartmentWithPagesAsync(int departmentId)
        {
            return await _dbSet
                .Include(d => d.Pages.Where(p => p.IsActive))
                    .ThenInclude(p => p.Layout)
                .FirstOrDefaultAsync(d => d.DepartmentID == departmentId);
        }

        public async Task<Department?> GetDepartmentWithSchedulesAsync(int departmentId)
        {
            return await _dbSet
                .Include(d => d.Schedules.Where(s => s.IsActive))
                .FirstOrDefaultAsync(d => d.DepartmentID == departmentId);
        }

        public async Task<Department?> GetDepartmentWithContentsAsync(int departmentId)
        {
            return await _dbSet
                .Include(d => d.Contents.Where(c => c.IsActive))
                .FirstOrDefaultAsync(d => d.DepartmentID == departmentId);
        }

        public async Task<Department?> GetDepartmentFullDetailsAsync(int departmentId)
        {
            return await _dbSet
                .Include(d => d.Company)
                .Include(d => d.Pages)
                    .ThenInclude(p => p.Layout)
                .Include(d => d.Schedules)
                .Include(d => d.Contents)
                .FirstOrDefaultAsync(d => d.DepartmentID == departmentId);
        }

        public async Task<IEnumerable<Department>> GetActiveDepartmentsByCompanyAsync(int companyId)
        {
            return await _dbSet.AsNoTracking()
                .Where(d => d.CompanyID == companyId && d.IsActive)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        public async Task<PagedResult<Department>> GetDepartmentsPagedAsync(
            int companyId, int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            IQueryable<Department> query = _dbSet.AsNoTracking()
                .Where(d => d.CompanyID == companyId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(d =>
                    d.DepartmentName.ToLower().Contains(term) ||
                    d.DepartmentCode.ToLower().Contains(term) ||
                    (d.Description != null && d.Description.ToLower().Contains(term)));
            }

            if (isActive.HasValue)
                query = query.Where(d => d.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(d => d.DepartmentName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Department>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
