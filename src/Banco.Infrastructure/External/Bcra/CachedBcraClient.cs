using Banco.Application.Interfaces.External;
using Banco.Domain.Bcra;
using Banco.Domain.ValueObjects;
using Banco.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Banco.Infrastructure.External.Bcra;

public class CachedBcraClient : IBcraClient
{
    private readonly IBcraClient _inner;
    private readonly IMemoryCache _cache;
    private readonly CacheOptions _options;
    private readonly ILogger<CachedBcraClient> _logger;

    public CachedBcraClient(IBcraClient inner, IMemoryCache cache, IOptions<CacheOptions> options, ILogger<CachedBcraClient> logger)
    {
        _inner = inner;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<DebtReport?> GetDebtReportAsync(Cuit cuit, CancellationToken cancellationToken)
    {
        var key = $"bcra:{cuit.Value}";

        if (_cache.TryGetValue(key, out CachedReport? cached) && cached is not null)
        {
            _logger.LogInformation("BCRA cache hit");
            return cached.Report;
        }

        _logger.LogInformation("BCRA cache miss");

        var report = await _inner.GetDebtReportAsync(cuit, cancellationToken);

        _cache.Set(key, new CachedReport(report), TimeSpan.FromSeconds(_options.BcraExpirationSeconds));

        return report;
    }

    // Wrapper so that "no data" (null) is cached too and can be told apart from "not in cache"
    private sealed record CachedReport(DebtReport? Report);
}