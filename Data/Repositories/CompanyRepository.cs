using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(AppDbContext context) : base(context) { }

        public async Task<Company?> GetByCompanyCodeAsync(string companyCode)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyCode == companyCode);
        }

        public async Task<Company?> GetCompanyWithDepartmentsAsync(int companyId)
        {
            return await _dbSet
                .Include(c => c.Departments.Where(d => d.IsActive))
                .FirstOrDefaultAsync(c => c.CompanyID == companyId);
        }

        public async Task<Company?> GetCompanyWithLayoutsAsync(int companyId)
        {
            return await _dbSet
                .Include(c => c.Layouts.Where(l => l.IsActive))
                .FirstOrDefaultAsync(c => c.CompanyID == companyId);
        }

        public async Task<Company?> GetCompanyWithConfigurationAsync(int companyId)
        {
            return await _dbSet
                .Include(c => c.Configuration)
                .FirstOrDefaultAsync(c => c.CompanyID == companyId);
        }

        public async Task<Company?> GetCompanyFullDetailsAsync(int companyId)
        {
            return await _dbSet
                .Include(c => c.Departments)
                .Include(c => c.Layouts)
                    .ThenInclude(l => l.LayoutSections)
                .Include(c => c.UserCompanyRoles)
                    .ThenInclude(ucr => ucr.User)
                .Include(c => c.Configuration)
                .FirstOrDefaultAsync(c => c.CompanyID == companyId);
        }

        public async Task<IEnumerable<Company>> GetActiveCompaniesAsync()
        {
            return await _dbSet.AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.CompanyName)
                .ToListAsync();
        }

        public async Task<PagedResult<Company>> GetCompaniesPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            IQueryable<Company> query = _dbSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(c =>
                    c.CompanyName.ToLower().Contains(term) ||
                    c.CompanyCode.ToLower().Contains(term) ||
                    (c.Description != null && c.Description.ToLower().Contains(term)));
            }

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CompanyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Company>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> IsCompanyCodeTakenAsync(string companyCode, int? excludeCompanyId = null)
        {
            return await _dbSet.AnyAsync(c =>
                c.CompanyCode == companyCode &&
                (!excludeCompanyId.HasValue || c.CompanyID != excludeCompanyId.Value));
        }
    }
}
