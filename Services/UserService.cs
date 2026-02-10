using DigitalSignage.Models;
using DigitalSignage.Data.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DigitalSignage.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User> CreateAsync(User entity)
        {
            // Burada şifre hashleme vb. business logic olacak
            return await _userRepository.AddAsync(entity);
        }

        public async Task<User> UpdateAsync(User entity)
        {
            entity.ModifiedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(entity);
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _userRepository.DeleteAsync(id);
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _userRepository.GetByUserNameAsync(userName);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<bool> AuthenticateAsync(string userName, string password)
        {
            var user = await GetByUserNameAsync(userName);
            if (user == null || !user.IsActive)
                return false;

            // TODO: Password Hashing implementation (BCrypt etc.)
            // Şimdilik plain text kontrolü (SECURITY RISK - DEV ONLY)
            return user.PasswordHash == password;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await GetByIdAsync(userId);
            if (user == null) return false;

            // Verify current password logic here
            // TODO: Implement proper hash verification
            if (user.PasswordHash != currentPassword) return false;

            user.PasswordHash = newPassword; // TODO: Should be hashed
            user.ModifiedDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return true;
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _userRepository.GetActiveUsersAsync();
        }

        public async Task<IEnumerable<UserCompanyRole>> GetUserCompaniesAsync(int userId)
        {
            var user = await _userRepository.Query()
                .Include(u => u.UserCompanyRoles)
                .ThenInclude(ucr => ucr.Company)
                .FirstOrDefaultAsync(u => u.UserID == userId);

            return user?.UserCompanyRoles ?? new List<UserCompanyRole>();
        }
    }
}
