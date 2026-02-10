using DigitalSignage.Models;
using DigitalSignage.Models.Common;
using DigitalSignage.Data;
using DigitalSignage.Helpers;

namespace DigitalSignage.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Users.GetByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _unitOfWork.Users.GetAllAsync();
        }

        public async Task<User> CreateAsync(User entity)
        {
            if (!string.IsNullOrEmpty(entity.PasswordHash))
            {
                entity.PasswordHash = PasswordHelper.HashPassword(entity.PasswordHash);
            }

            entity.CreatedDate = DateTime.UtcNow;
            await _unitOfWork.Users.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User created: {UserName}", entity.UserName);
            return entity;
        }

        public async Task<User> UpdateAsync(User entity)
        {
            entity.ModifiedDate = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _unitOfWork.Users.GetByUserNameAsync(userName);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetByEmailAsync(email);
        }

        public async Task<User?> AuthenticateAsync(string userName, string password)
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(userName);
            if (user == null || !user.IsActive)
                return null;

            if (string.IsNullOrEmpty(user.PasswordHash))
                return null;

            if (!PasswordHelper.VerifyPassword(password, user.PasswordHash))
                return null;

            user.LastLoginDate = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User authenticated: {UserName}", userName);
            return user;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return false;

            if (!PasswordHelper.VerifyPassword(currentPassword, user.PasswordHash))
                return false;

            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            user.ModifiedDate = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Password changed for user: {UserId}", userId);
            return true;
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _unitOfWork.Users.GetActiveUsersAsync();
        }

        public async Task<User?> GetUserWithRolesAsync(int userId)
        {
            return await _unitOfWork.Users.GetUserWithRolesAsync(userId);
        }

        public async Task<IEnumerable<User>> GetUsersByCompanyAsync(int companyId)
        {
            return await _unitOfWork.Users.GetUsersByCompanyAsync(companyId);
        }

        public async Task<PagedResult<User>> GetUsersPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            return await _unitOfWork.Users.GetUsersPagedAsync(pageNumber, pageSize, searchTerm, isActive);
        }
    }
}
