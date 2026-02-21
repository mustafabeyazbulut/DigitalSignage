using DigitalSignage.Services;

namespace DigitalSignage.Middleware
{
    public class TenantResolverMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantResolverMiddleware> _logger;

        public TenantResolverMiddleware(RequestDelegate next, ILogger<TenantResolverMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuthorizationService authService)
        {
            // Sadece kimlik doğrulanmış kullanıcılar için tenant context belirle
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            int? companyId = null;

            // Öncelik 1: Route parametresinden company ID al
            var companyIdRoute = context.GetRouteValue("companyId")?.ToString();
            if (companyIdRoute != null && int.TryParse(companyIdRoute, out var routeCompanyId))
            {
                companyId = routeCompanyId;
            }

            // Öncelik 2: Session'dan fallback
            if (companyId == null)
            {
                var sessionCompanyId = context.Session.GetInt32("SelectedCompanyId");
                if (sessionCompanyId.HasValue)
                {
                    companyId = sessionCompanyId.Value;
                }
            }

            // Context'e set et (yetki kontrolü service/controller katmanında yapılacak)
            // NOT: Burada yetki kontrolü yapılmaz çünkü SystemAdmin tüm şirketlere erişebilir
            // Authorization kontrolü AuthorizationService ve Controller'larda yapılır
            if (companyId.HasValue)
            {
                context.Items["CompanyId"] = companyId.Value;
                _logger.LogDebug($"Tenant context set: CompanyId={companyId}");
            }

            await _next(context);
        }
    }
}
