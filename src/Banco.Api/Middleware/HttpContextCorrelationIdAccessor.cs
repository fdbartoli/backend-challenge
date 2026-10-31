using Banco.Application.Interfaces.Context;

namespace Banco.Api.Middleware;

public class HttpContextCorrelationIdAccessor : ICorrelationIdAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? CorrelationId
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                return null;
            }

            return httpContext.Items[CorrelationIdMiddleware.ItemKey] as string;
        }
    }
}