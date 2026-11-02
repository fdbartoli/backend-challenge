using Banco.Application.Common.Exceptions;
using Banco.Application.Interfaces.External;
using Banco.Domain.Bcra;
using Banco.Domain.ValueObjects;
using Banco.Infrastructure.Caching;
using Banco.Infrastructure.External.Bcra;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Banco.Tests.Infrastructure;

public class CachedBcraClientTests
{
    private static readonly Cuit ArcosDorados = new("30610252334");
    private static readonly Cuit Lucia = new("27990000021");

    private readonly Mock<IBcraClient> _innerClient = new();

    private CachedBcraClient CreateCachedClient()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(new CacheOptions { BcraExpirationSeconds = 300 });

        return new CachedBcraClient(_innerClient.Object, cache, options, NullLogger<CachedBcraClient>.Instance);
    }

    private static DebtReport CreateReport()
    {
        return new DebtReport("202609", new List<DebtEntry> { new("BANCO A", 1, 1500m, 0) });
    }

    [Fact]
    public async Task GetDebtReportAsync_FirstCall_CallsInnerClient()
    {
        var report = CreateReport();
        _innerClient
            .Setup(c => c.GetDebtReportAsync(ArcosDorados, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
        var cachedClient = CreateCachedClient();

        var result = await cachedClient.GetDebtReportAsync(ArcosDorados, CancellationToken.None);

        Assert.Same(report, result);
        _innerClient.Verify(c => c.GetDebtReportAsync(ArcosDorados, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDebtReportAsync_SecondCallWithSameCuit_ReturnsCachedReport()
    {
        var report = CreateReport();
        _innerClient
            .Setup(c => c.GetDebtReportAsync(ArcosDorados, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
        var cachedClient = CreateCachedClient();

        await cachedClient.GetDebtReportAsync(ArcosDorados, CancellationToken.None);
        var second = await cachedClient.GetDebtReportAsync(ArcosDorados, CancellationToken.None);

        Assert.Same(report, second);
        _innerClient.Verify(c => c.GetDebtReportAsync(ArcosDorados, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDebtReportAsync_WhenInnerReturnsNoData_CachesTheNoDataResult()
    {
        _innerClient
            .Setup(c => c.GetDebtReportAsync(Lucia, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DebtReport?)null);
        var cachedClient = CreateCachedClient();

        var first = await cachedClient.GetDebtReportAsync(Lucia, CancellationToken.None);
        var second = await cachedClient.GetDebtReportAsync(Lucia, CancellationToken.None);

        Assert.Null(first);
        Assert.Null(second);
        _innerClient.Verify(c => c.GetDebtReportAsync(Lucia, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDebtReportAsync_WhenInnerFails_DoesNotCacheTheFailure()
    {
        _innerClient
            .Setup(c => c.GetDebtReportAsync(ArcosDorados, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExternalServiceUnavailableException("BCRA service is unavailable.", new HttpRequestException()));
        var cachedClient = CreateCachedClient();

        await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
            () => cachedClient.GetDebtReportAsync(ArcosDorados, CancellationToken.None));
        await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
            () => cachedClient.GetDebtReportAsync(ArcosDorados, CancellationToken.None));

        _innerClient.Verify(c => c.GetDebtReportAsync(ArcosDorados, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetDebtReportAsync_WithDifferentCuits_CallsInnerClientForEach()
    {
        _innerClient
            .Setup(c => c.GetDebtReportAsync(It.IsAny<Cuit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateReport());
        var cachedClient = CreateCachedClient();

        await cachedClient.GetDebtReportAsync(ArcosDorados, CancellationToken.None);
        await cachedClient.GetDebtReportAsync(Lucia, CancellationToken.None);

        _innerClient.Verify(c => c.GetDebtReportAsync(It.IsAny<Cuit>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}