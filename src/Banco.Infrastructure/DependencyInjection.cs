using Banco.Application.Interfaces.External;
using Banco.Application.Interfaces.Persistence;
using Banco.Infrastructure.Caching;
using Banco.Infrastructure.External.Bcra;
using Banco.Infrastructure.External.Http;
using Banco.Infrastructure.Persistence;
using Banco.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Banco.Infrastructure.HealthChecks;

namespace Banco.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "Database:ConnectionString is required.")
            .ValidateOnStart();

        services.AddDbContext<BancoDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            options.UseSqlite(databaseOptions.ConnectionString);
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();

        services.AddOptions<BcraOptions>()
            .Bind(configuration.GetSection(BcraOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _) && options.BaseUrl.EndsWith('/'),
                "Bcra:BaseUrl must be an absolute URL ending with '/'.")
            .Validate(options => options.TimeoutSeconds > 0,
                "Bcra:TimeoutSeconds must be greater than zero.")
            .Validate(options => options.Resilience.RetryCount >= 1,
                "Bcra:Resilience:RetryCount must be at least 1.")
            .Validate(options => options.Resilience.CircuitBreakerFailureRatio > 0 && options.Resilience.CircuitBreakerFailureRatio <= 1,
                "Bcra:Resilience:CircuitBreakerFailureRatio must be between 0 and 1.")
            .Validate(options => options.Resilience.CircuitBreakerMinimumThroughput >= 2,
                "Bcra:Resilience:CircuitBreakerMinimumThroughput must be at least 2.")
            .ValidateOnStart();

        services.AddMemoryCache();

        services.AddOptions<CacheOptions>()
            .Bind(configuration.GetSection(CacheOptions.SectionName))
            .Validate(options => options.BcraExpirationSeconds > 0,
                "Cache:BcraExpirationSeconds must be greater than zero.")
            .ValidateOnStart();

        services.AddTransient<CorrelationIdDelegatingHandler>();

        // Shared so the health check can read the circuit state
        services.AddSingleton<CircuitBreakerStateProvider>();

        services.AddHttpClient<BcraClient>((serviceProvider, client) =>
        {
            var bcraOptions = serviceProvider.GetRequiredService<IOptions<BcraOptions>>().Value;
            client.BaseAddress = new Uri(bcraOptions.BaseUrl);
        })
        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
        .AddResilienceHandler("bcra", (builder, context) =>
        {
            var bcraOptions = context.ServiceProvider.GetRequiredService<IOptions<BcraOptions>>().Value;
            var resilience = bcraOptions.Resilience;

            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = resilience.RetryCount,
                Delay = TimeSpan.FromMilliseconds(resilience.RetryBaseDelayMilliseconds),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            });

            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = resilience.CircuitBreakerFailureRatio,
                MinimumThroughput = resilience.CircuitBreakerMinimumThroughput,
                SamplingDuration = TimeSpan.FromSeconds(resilience.CircuitBreakerSamplingDurationSeconds),
                BreakDuration = TimeSpan.FromSeconds(resilience.CircuitBreakerBreakDurationSeconds),
                StateProvider = context.ServiceProvider.GetRequiredService<CircuitBreakerStateProvider>()
            });

            builder.AddTimeout(TimeSpan.FromSeconds(bcraOptions.TimeoutSeconds));
        });

        services.AddScoped<IBcraClient>(serviceProvider => new CachedBcraClient(
            serviceProvider.GetRequiredService<BcraClient>(),
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<IOptions<CacheOptions>>(),
            serviceProvider.GetRequiredService<ILogger<CachedBcraClient>>()));

        services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database")
        .AddCheck<BcraCircuitHealthCheck>("bcra");

        return services;
    }
}