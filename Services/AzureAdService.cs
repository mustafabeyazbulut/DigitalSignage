using System.Net.Http.Headers;
using System.Text.Json;
using DigitalSignage.DTOs;

namespace DigitalSignage.Services
{
    public class AzureAdService : IAzureAdService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureAdService> _logger;

        public AzureAdService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<AzureAdService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UserProfileDTO> GetUserProfileAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{_configuration["MicrosoftGraph:BaseUrl"]}/me");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var profile = JsonSerializer.Deserialize<UserProfileDTO>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation($"User profile retrieved: {profile?.Mail}");
                return profile ?? new UserProfileDTO();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                throw;
            }
        }

        public async Task<List<string>> GetUserGroupsAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{_configuration["MicrosoftGraph:BaseUrl"]}/me/memberOf");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(content);

                var groups = new List<string>();
                foreach (var item in result.RootElement.GetProperty("value").EnumerateArray())
                {
                    if (item.TryGetProperty("displayName", out var name))
                    {
                        var displayName = name.GetString();
                        if (!string.IsNullOrEmpty(displayName))
                        {
                            groups.Add(displayName);
                        }
                    }
                }

                _logger.LogInformation($"User groups retrieved: {string.Join(", ", groups)}");
                return groups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user groups");
                throw;
            }
        }

        public async Task<UserEmailDTO?> GetUserEmailAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{_configuration["MicrosoftGraph:BaseUrl"]}/me/messages?$top=1");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<UserEmailDTO>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user email");
                return null;
            }
        }

        public async Task<UserPhotoDTO?> GetUserPhotoAsync(string accessToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get,
                    $"{_configuration["MicrosoftGraph:BaseUrl"]}/me/photo/$value");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var photoBytes = await response.Content.ReadAsByteArrayAsync();
                    return new UserPhotoDTO
                    {
                        PhotoBytes = photoBytes,
                        ContentType = response.Content.Headers.ContentType?.MediaType
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user photo");
                return null;
            }
        }
    }
}
