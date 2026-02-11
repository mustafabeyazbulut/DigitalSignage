using DigitalSignage.Services;

namespace DigitalSignage.Middleware
{
    public class TenantResolverMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolverMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // URL'den company ID al
            var companyIdRoute = context.GetRouteValue("companyId")?.ToString();

            if (companyIdRoute != null && int.TryParse(companyIdRoute, out var companyId))
            {
                context.Items["CompanyId"] = companyId;
            }

            // Veya session'dan al
            var sessionCompanyId = context.Session.GetInt32("SelectedCompanyId");
            if (sessionCompanyId.HasValue && context.Items["CompanyId"] == null)
            {
                context.Items["CompanyId"] = sessionCompanyId.Value;
            }

            await _next(context);
        }
    }
}
