using DigitalSignage.Models;

namespace DigitalSignage.Services
{
    public interface IUserService : IService<User>
    {
        Task<User?> GetByUserNameAsync(string userName);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> AuthenticateAsync(string userName, string password);
        // Task<User> CreateUserAsync(CreateUserDTO dto); -- DTO'lar henüz oluşturulmadı, sonra eklenecek
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<IEnumerable<User>> GetActiveUsersAsync();
        Task<IEnumerable<UserCompanyRole>> GetUserCompaniesAsync(int userId);
    }
}
