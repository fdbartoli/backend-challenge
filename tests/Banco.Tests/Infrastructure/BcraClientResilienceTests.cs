using System.Net;
using System.Text;
using Banco.Application.Common.Exceptions;
using Banco.Application.Interfaces.Context;
using Banco.Domain.ValueObjects;
using Banco.Infrastructure;
using Banco.Infrastructure.External.Bcra;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Polly.CircuitBreaker;

namespace Banco.Tests.Infrastructure;

public class BcraClientResilienceTests
{
    private const string CorrelationId = "test-correlation-id";
    private static readonly Cuit ArcosDorados = new("30610252334");

    private const string ReportJson = """
        {"status":200,"results":{"identificacion":30610252334,"denominacion":"ARCOS DORADOS","periodos":[
          {"periodo":"202608","entidades":[{"entidad":"BANCO A","situacion":2,"monto":10.0,"diasAtrasoPago":31}]},
          {"periodo":"202609","entidades":[
            {"entidad":"BANCO A","situacion":1,"monto":20.0,"diasAtrasoPago":0},
            {"entidad":"BANCO B","situacion":3,"monto":5.0,"diasAtrasoPago":95}]}]}}
        """;

    private readonly Mock<HttpMessageHandler> _handler = new();

    private ServiceProvider BuildServices(int timeoutSeconds = 5)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = "Data Source=:memory:",
                ["Bcra:BaseUrl"] = "https://bcra.test/centraldedeudores/v1.0/",
                ["Bcra:TimeoutSeconds"] = timeoutSeconds.ToString(),
                ["Bcra:Resilience:RetryCount"] = "2",
                ["Bcra:Resilience:RetryBaseDelayMilliseconds"] = "1",
                ["Bcra:Resilience:CircuitBreakerFailureRatio"] = "0.5",
                ["Bcra:Resilience:CircuitBreakerMinimumThroughput"] = "5",
                ["Bcra:Resilience:CircuitBreakerSamplingDurationSeconds"] = "30",
                ["Bcra:Resilience:CircuitBreakerBreakDurationSeconds"] = "30",
                ["Cache:BcraExpirationSeconds"] = "300"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Mock.Of<ICorrelationIdAccessor>(a => a.CorrelationId == CorrelationId));
        services.AddInfrastructure(configuration);

        // Same typed client as the app, but the last handler is the mock instead of the network
        services.AddHttpClient<BcraClient>().ConfigurePrimaryHttpMessageHandler(() => _handler.Object);

        return services.BuildServiceProvider();
    }

    private void RespondAlways(HttpStatusCode statusCode, string? content = null)
    {
        _handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => CreateResponse(statusCode, content));
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string? content)
    {
        var response = new HttpResponseMessage(statusCode);

        if (content is not null)
        {
            response.Content = new StringContent(content, Encoding.UTF8, "application/json");
        }

        return response;
    }

    private void VerifyAttempts(int expected)
    {
        _handler.Protected().Verify("SendAsync", Times.Exactly(expected),
            ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetDebtReportAsync_WhenBcraAnswers200_ReturnsLatestPeriod()
    {
        RespondAlways(HttpStatusCode.OK, ReportJson);
        using var services = BuildServices();
        var client = services.GetRequiredService<BcraClient>();

        var report = await client.GetDebtReportAsync(ArcosDorados, CancellationToken.None);

        Assert.NotNull(report);
        Assert.Equal("202609", report.Period);
        Assert.Equal(2, report.Entries.Count);
        Assert.Equal(3, report.WorstSituation);
        VerifyAttempts(1);
    }

    [Fact]
    public async Task GetDebtReportAsync_WhenBcraAnswers404_ReturnsNullWithoutRetrying()
    {
        RespondAlways(HttpStatusCode.NotFound, """{"status":404,"errorMessages":["No se encontró datos"]}""");
        using var services = BuildServices();
        var client = services.GetRequiredService<BcraClient>();

        var report = await client.GetDebtReportAsync(ArcosDorados, CancellationToken.None);

        Assert.Null(report);
        VerifyAttempts(1);
    }

    [Fact]
    public async Task GetDebtReportAsync_WhenBcraAnswers500_RetriesAndThrowsUnavailable()
    {
        RespondAlways(HttpStatusCode.InternalServerError);
        using var services = BuildServices();
        var client = services.GetRequiredService<BcraClient>();

        await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
            () => client.GetDebtReportAsync(ArcosDorados, CancellationToken.None));

        VerifyAttempts(3);
    }

    [Fact]
    public async Task GetDebtReportAsync_WhenFirstAttemptFails_RetriesAndReturnsReport()
    {
        _handler.Protected()
            .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(CreateResponse(HttpStatusCode.ServiceUnavailable, null))
            .ReturnsAsync(CreateResponse(HttpStatusCode.OK, ReportJson));
        using var services = BuildServices();
        var client = services.GetRequiredService<BcraClient>();

        var report = await client.GetDebtReportAsync(ArcosDorados, CancellationToken.None);

        Assert.NotNull(report);
        VerifyAttempts(2);
    }

    [Fact]
    public async Task GetDebtReportAsync_AfterRepeatedFailures_OpensCircuitAndStopsCalling()
    {
        RespondAlways(HttpStatusCode.InternalServerError);
        using var services = BuildServices();
        var client = services.GetRequiredService<BcraClient>();

        for (var i = 0; i < 3; i++)
        {
            await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
                () => client.GetDebtReportAsync(ArcosDorados, CancellationToken.None));
        }

        // 3 attempts on the first call, 2 on the second (the circuit opens on the 5th failure), 0 on the third
        VerifyAttempts(5);
        Assert.Equal(CircuitState.Open, services.GetRequiredService<CircuitBreakerStateProvider>().CircuitState);
    }

    [Fact]
    public async Task GetDebtReportAsync_WhenBcraIsTooSlow_TimesOutAndThrowsUnavailable()
    {
        _handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns<HttpRequestMessage, CancellationToken>(async (request, token) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(30), token);
                return CreateResponse(HttpStatusCode.OK, ReportJson);
            });
        using var services = BuildServices(timeoutSeconds: 1);
        var client = services.GetRequiredService<BcraClient>();

        await Assert.ThrowsAsync<ExternalServiceUnavailableException>(
            () => client.GetDebtReportAsync(ArcosDorados, CancellationToken.None));

        VerifyAttempts(3);
    }

    [Fact]
    public async Task GetDebtReportAsync_Always_SendsCorrelationIdHeader()
    {
        HttpRequestMessage? sentRequest = null;
        _handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, token) => sentRequest = request)
            .ReturnsAsync(() => CreateResponse(HttpStatusCode.NotFound, null));
        using var services = BuildServices();
        var client = services.GetRequiredService<BcraClient>();

        await client.GetDebtReportAsync(ArcosDorados, CancellationToken.None);

        Assert.NotNull(sentRequest);
        Assert.Equal(CorrelationId, sentRequest.Headers.GetValues("X-Correlation-ID").Single());
        Assert.Equal("/centraldedeudores/v1.0/deudas/30610252334", sentRequest.RequestUri!.AbsolutePath);
    }
}