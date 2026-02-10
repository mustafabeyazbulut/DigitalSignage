using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public class PageRepository : Repository<Page>, IPageRepository
    {
        public PageRepository(AppDbContext context) : base(context) { }
    }
}
