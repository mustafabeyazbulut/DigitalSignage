using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public class DepartmentRepository : Repository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context) { }
    }
}
