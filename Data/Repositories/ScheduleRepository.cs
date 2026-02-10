using DigitalSignage.Models;

namespace DigitalSignage.Data.Repositories
{
    public class ScheduleRepository : Repository<Schedule>, IScheduleRepository
    {
        public ScheduleRepository(AppDbContext context) : base(context) { }
    }
}
