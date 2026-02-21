using DigitalSignage.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalSignage.Data.Repositories
{
    /// <summary>
    /// UserDepartmentRole repository implementation
    /// </summary>
    public class UserDepartmentRoleRepository : Repository<UserDepartmentRole>, IUserDepartmentRoleRepository
    {
        public UserDepartmentRoleRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<UserDepartmentRole>> GetUserDepartmentRolesAsync(int userId)
        {
            return await _dbSet
                .Include(udr => udr.Department)
                .Include(udr => udr.User)
                .Where(udr => udr.UserID == userId && udr.IsActive)
                .OrderBy(udr => udr.Department.DepartmentName)
                .ToListAsync();
        }

        public async Task<List<UserDepartmentRole>> GetDepartmentUsersAsync(int departmentId)
        {
            return await _dbSet
                .Include(udr => udr.User)
                .Include(udr => udr.Department)
                .Where(udr => udr.DepartmentID == departmentId && udr.IsActive)
                .OrderBy(udr => udr.User.Email)
                .ToListAsync();
        }

        public async Task<UserDepartmentRole?> GetUserDepartmentRoleAsync(int userId, int departmentId)
        {
            return await _dbSet
                .Include(udr => udr.User)
                .Include(udr => udr.Department)
                .FirstOrDefaultAsync(udr =>
                    udr.UserID == userId &&
                    udr.DepartmentID == departmentId &&
                    udr.IsActive);
        }

        public async Task<bool> CheckUserDepartmentAccessAsync(int userId, int departmentId)
        {
            return await _dbSet
                .AnyAsync(udr =>
                    udr.UserID == userId &&
                    udr.DepartmentID == departmentId &&
                    udr.IsActive);
        }

        public async Task<UserDepartmentRole> AssignRoleAsync(int userId, int departmentId, string role, string assignedBy)
        {
            // Mevcut rol var mı kontrol et
            var existingRole = await GetUserDepartmentRoleAsync(userId, departmentId);

            if (existingRole != null)
            {
                // Rolü güncelle
                existingRole.Role = role;
                existingRole.AssignedDate = DateTime.UtcNow;
                existingRole.AssignedBy = assignedBy;
                existingRole.IsActive = true;

                await UpdateAsync(existingRole);
                return existingRole;
            }
            else
            {
                // Yeni rol ekle
                var newRole = new UserDepartmentRole
                {
                    UserID = userId,
                    DepartmentID = departmentId,
                    Role = role,
                    IsActive = true,
                    AssignedDate = DateTime.UtcNow,
                    AssignedBy = assignedBy
                };

                return await AddAsync(newRole);
            }
        }

        public async Task<bool> RemoveRoleAsync(int userId, int departmentId)
        {
            var role = await GetUserDepartmentRoleAsync(userId, departmentId);

            if (role != null)
            {
                await DeleteAsync(role.UserDepartmentRoleID);
                return true;
            }

            return false;
        }
    }
}
