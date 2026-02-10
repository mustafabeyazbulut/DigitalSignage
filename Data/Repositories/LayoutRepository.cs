using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public class LayoutRepository : Repository<Layout>, ILayoutRepository
    {
        public LayoutRepository(AppDbContext context) : base(context) { }
    }
}
