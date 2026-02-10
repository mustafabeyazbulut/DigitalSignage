using DigitalSignage.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Data.Repositories
{
    public class CompanyConfigurationRepository : Repository<CompanyConfiguration>, ICompanyConfigurationRepository
    {
        public CompanyConfigurationRepository(AppDbContext context) : base(context) { }

        public async Task<CompanyConfiguration?> GetByCompanyAsync(int companyId)
        {
            return await _dbSet
                .Include(cc => cc.Company)
                .FirstOrDefaultAsync(cc => cc.CompanyID == companyId);
        }

        public async Task<CompanyConfiguration> GetOrCreateByCompanyAsync(int companyId)
        {
            var config = await _dbSet
                .FirstOrDefaultAsync(cc => cc.CompanyID == companyId);

            if (config == null)
            {
                config = new CompanyConfiguration { CompanyID = companyId };
                await _dbSet.AddAsync(config);
            }

            return config;
        }
    }
}
