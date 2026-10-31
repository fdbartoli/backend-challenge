using Banco.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Banco.Infrastructure.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly BancoDbContext _dbContext;

    public DatabaseHealthCheck(BancoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

        if (canConnect)
        {
            return HealthCheckResult.Healthy("SQLite database is reachable.");
        }

        return HealthCheckResult.Unhealthy("SQLite database is not reachable.");
    }
}