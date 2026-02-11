using System.Security.Claims;

namespace DigitalSignage.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetObjectId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        }

        public static string? GetTenantId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
        }

        public static string? GetMail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Upn)?.Value ??
                   principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string? GetDisplayName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string? GetGivenName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.GivenName)?.Value;
        }

        public static string? GetSurname(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Surname)?.Value;
        }

        public static string? GetPhotoUrl(this ClaimsPrincipal principal)
        {
            return principal.FindFirst("picture")?.Value;
        }

        public static List<string> GetRoles(this ClaimsPrincipal principal)
        {
            return principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        }
    }
}
