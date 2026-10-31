using Banco.Application.Interfaces.Context;

namespace Banco.Infrastructure.External.Http;

public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private const string HeaderName = "X-Correlation-ID";

    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public CorrelationIdDelegatingHandler(ICorrelationIdAccessor correlationIdAccessor)
    {
        _correlationIdAccessor = correlationIdAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = _correlationIdAccessor.CorrelationId;

        if (!string.IsNullOrEmpty(correlationId) && !request.Headers.Contains(HeaderName))
        {
            request.Headers.Add(HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}