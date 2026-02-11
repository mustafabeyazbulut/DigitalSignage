using DigitalSignage.DTOs;

namespace DigitalSignage.Services
{
    public interface IAzureAdService
    {
        Task<UserProfileDTO> GetUserProfileAsync(string accessToken);
        Task<List<string>> GetUserGroupsAsync(string accessToken);
        Task<UserEmailDTO?> GetUserEmailAsync(string accessToken);
        Task<UserPhotoDTO?> GetUserPhotoAsync(string accessToken);
    }
}
