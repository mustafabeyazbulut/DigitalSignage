using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public class ContentRepository : Repository<Content>, IContentRepository
    {
        public ContentRepository(AppDbContext context) : base(context) { }
    }
}
