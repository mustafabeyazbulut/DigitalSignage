using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(AppDbContext context) : base(context) { }
    }
}
