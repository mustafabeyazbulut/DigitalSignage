using Microsoft.EntityFrameworkCore;
using DigitalSignage.Models;

namespace DigitalSignage.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Layout> Layouts { get; set; }
        public DbSet<LayoutSection> LayoutSections { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<Content> Contents { get; set; }
        public DbSet<PageContent> PageContents { get; set; }
        public DbSet<PageSection> PageSections { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<SchedulePage> SchedulePages { get; set; }
        public DbSet<UserCompanyRole> UserCompanyRoles { get; set; }
        public DbSet<UserDepartmentRole> UserDepartmentRoles { get; set; }
        public DbSet<CompanyConfiguration> CompanyConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === User Configuration ===
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // === Company Configuration ===
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.CompanyID);
                entity.HasIndex(e => e.CompanyCode).IsUnique();

                // 1:1 Config
                entity.HasOne(e => e.Configuration)
                    .WithOne(c => c.Company)
                    .HasForeignKey<CompanyConfiguration>(c => c.CompanyID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // === CompanyConfiguration Entity ===
            modelBuilder.Entity<CompanyConfiguration>(entity =>
            {
                entity.HasKey(e => e.ConfigurationID);
            });

            // === Department Configuration ===
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DepartmentID);
                entity.HasOne(d => d.Company)
                    .WithMany(c => c.Departments)
                    .HasForeignKey(d => d.CompanyID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // === User Company Role ===
            modelBuilder.Entity<UserCompanyRole>(entity =>
            {
                entity.HasKey(e => e.UserCompanyRoleID);
                entity.HasOne(ucr => ucr.User)
                    .WithMany(u => u.UserCompanyRoles)
                    .HasForeignKey(ucr => ucr.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ucr => ucr.Company)
                    .WithMany(c => c.UserCompanyRoles)
                    .HasForeignKey(ucr => ucr.CompanyID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique index: Bir kullanıcı bir şirkette sadece bir role sahip olabilir
                entity.HasIndex(e => new { e.UserID, e.CompanyID }).IsUnique();
            });

            // === User Department Role ===
            modelBuilder.Entity<UserDepartmentRole>(entity =>
            {
                entity.HasKey(e => e.UserDepartmentRoleID);
                entity.HasOne(udr => udr.User)
                    .WithMany(u => u.UserDepartmentRoles)
                    .HasForeignKey(udr => udr.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(udr => udr.Department)
                    .WithMany(d => d.UserDepartmentRoles)
                    .HasForeignKey(udr => udr.DepartmentID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Unique index: Bir kullanıcı bir departmanda sadece bir role sahip olabilir
                entity.HasIndex(e => new { e.UserID, e.DepartmentID }).IsUnique();
            });

            // === Layout Configuration ===
            modelBuilder.Entity<Layout>(entity =>
            {
                entity.HasKey(e => e.LayoutID);
                entity.HasOne(l => l.Company)
                    .WithMany(c => c.Layouts)
                    .HasForeignKey(l => l.CompanyID)
                    .OnDelete(DeleteBehavior.NoAction); // Avoid multiple cascades
            });

            modelBuilder.Entity<LayoutSection>(entity =>
            {
                entity.HasKey(e => e.LayoutSectionID);
                entity.HasOne(ls => ls.Layout)
                    .WithMany(l => l.LayoutSections)
                    .HasForeignKey(ls => ls.LayoutID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // === Page Configuration ===
            modelBuilder.Entity<Page>(entity =>
            {
                entity.HasKey(e => e.PageID);
                entity.HasOne(p => p.Department)
                    .WithMany(d => d.Pages)
                    .HasForeignKey(p => p.DepartmentID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Layout)
                    .WithMany(l => l.Pages)
                    .HasForeignKey(p => p.LayoutID)
                    .OnDelete(DeleteBehavior.Restrict); // Layout silinirse sayfalar bozulmasın
            });

            // === Page Content N:N Configuration ===
            modelBuilder.Entity<PageContent>(entity =>
            {
                entity.HasKey(e => e.PageContentID);
                
                entity.HasOne(pc => pc.Page)
                    .WithMany(p => p.PageContents)
                    .HasForeignKey(pc => pc.PageID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Content)
                    .WithMany(c => c.PageContents)
                    .HasForeignKey(pc => pc.ContentID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // === Page Section Relation ===
            modelBuilder.Entity<PageSection>(entity =>
            {
                entity.HasKey(e => e.PageSectionID);

                entity.HasOne(ps => ps.Page)
                    .WithMany(p => p.PageSections)
                    .HasForeignKey(ps => ps.PageID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ps => ps.LayoutSection)
                    .WithMany(ls => ls.PageSections)
                    .HasForeignKey(ps => ps.LayoutSectionID)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // === Schedule Configuration ===
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.ScheduleID);
                
                entity.HasOne(s => s.Department)
                    .WithMany(d => d.Schedules)
                    .HasForeignKey(s => s.DepartmentID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SchedulePage>(entity =>
            {
                entity.HasKey(e => e.SchedulePageID);
                
                entity.HasOne(sp => sp.Schedule)
                    .WithMany(s => s.SchedulePages)
                    .HasForeignKey(sp => sp.ScheduleID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sp => sp.Page)
                    .WithMany(p => p.SchedulePages)
                    .HasForeignKey(sp => sp.PageID)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // === Content Configuration ===
            modelBuilder.Entity<Content>(entity =>
            {
                entity.HasKey(e => e.ContentID);
                
                entity.HasOne(c => c.Department)
                    .WithMany(d => d.Contents)
                    .HasForeignKey(c => c.DepartmentID)
                    .OnDelete(DeleteBehavior.NoAction); // Content silindiğinde page content'te sorun çıkmasın
            });
        }
    }
}
