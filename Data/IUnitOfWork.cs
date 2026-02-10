using DigitalSignage.Data.Repositories;

namespace DigitalSignage.Data
{
    public interface IUnitOfWork : IDisposable
    {
        // === Repository Properties ===
        IUserRepository Users { get; }
        ICompanyRepository Companies { get; }
        IDepartmentRepository Departments { get; }
        ILayoutRepository Layouts { get; }
        ILayoutSectionRepository LayoutSections { get; }
        IPageRepository Pages { get; }
        IContentRepository Contents { get; }
        IPageContentRepository PageContents { get; }
        IPageSectionRepository PageSections { get; }
        IScheduleRepository Schedules { get; }
        ISchedulePageRepository SchedulePages { get; }
        IUserCompanyRoleRepository UserCompanyRoles { get; }
        ICompanyConfigurationRepository CompanyConfigurations { get; }

        // === Transaction Management ===
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
