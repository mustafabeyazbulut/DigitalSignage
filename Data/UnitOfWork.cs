using Microsoft.EntityFrameworkCore.Storage;
using DigitalSignage.Data.Repositories;

namespace DigitalSignage.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        private IUserRepository? _users;
        private ICompanyRepository? _companies;
        private IDepartmentRepository? _departments;
        private ILayoutRepository? _layouts;
        private ILayoutSectionRepository? _layoutSections;
        private IPageRepository? _pages;
        private IContentRepository? _contents;
        private IPageContentRepository? _pageContents;
        private IPageSectionRepository? _pageSections;
        private IScheduleRepository? _schedules;
        private ISchedulePageRepository? _schedulePages;
        private IUserCompanyRoleRepository? _userCompanyRoles;
        private ICompanyConfigurationRepository? _companyConfigurations;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // === Lazy-initialized Repository Properties ===
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public ICompanyRepository Companies => _companies ??= new CompanyRepository(_context);
        public IDepartmentRepository Departments => _departments ??= new DepartmentRepository(_context);
        public ILayoutRepository Layouts => _layouts ??= new LayoutRepository(_context);
        public ILayoutSectionRepository LayoutSections => _layoutSections ??= new LayoutSectionRepository(_context);
        public IPageRepository Pages => _pages ??= new PageRepository(_context);
        public IContentRepository Contents => _contents ??= new ContentRepository(_context);
        public IPageContentRepository PageContents => _pageContents ??= new PageContentRepository(_context);
        public IPageSectionRepository PageSections => _pageSections ??= new PageSectionRepository(_context);
        public IScheduleRepository Schedules => _schedules ??= new ScheduleRepository(_context);
        public ISchedulePageRepository SchedulePages => _schedulePages ??= new SchedulePageRepository(_context);
        public IUserCompanyRoleRepository UserCompanyRoles => _userCompanyRoles ??= new UserCompanyRoleRepository(_context);
        public ICompanyConfigurationRepository CompanyConfigurations => _companyConfigurations ??= new CompanyConfigurationRepository(_context);

        // === Transaction Management ===
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                if (_transaction != null)
                    await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
