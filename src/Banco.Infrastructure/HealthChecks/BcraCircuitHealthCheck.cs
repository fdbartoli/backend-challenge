using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly.CircuitBreaker;

namespace Banco.Infrastructure.HealthChecks;

public class BcraCircuitHealthCheck : IHealthCheck
{
    private readonly CircuitBreakerStateProvider _circuitStateProvider;

    public BcraCircuitHealthCheck(CircuitBreakerStateProvider circuitStateProvider)
    {
        _circuitStateProvider = circuitStateProvider;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var state = _circuitStateProvider.CircuitState;

        var data = new Dictionary<string, object>
        {
            ["circuitState"] = state.ToString()
        };

        if (state == CircuitState.Closed)
        {
            return Task.FromResult(HealthCheckResult.Healthy("BCRA circuit is closed.", data));
        }

        return Task.FromResult(HealthCheckResult.Degraded($"BCRA circuit is {state}; credit situation is served as Unavailable.", data: data));
    }
}