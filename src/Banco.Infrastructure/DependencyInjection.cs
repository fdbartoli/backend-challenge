
using Banco.Application.Interfaces.Persistence;
using Banco.Infrastructure.Persistence;
using Banco.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Banco.Application.Interfaces.External;
using Banco.Infrastructure.External.Bcra;
using Banco.Infrastructure.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Banco.Infrastructure.External.Http;

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
            .ValidateOnStart();
        services.AddMemoryCache();
        services.AddOptions<CacheOptions>()
            .Bind(configuration.GetSection(CacheOptions.SectionName))
            .Validate(options => options.BcraExpirationSeconds > 0,
                "Cache:BcraExpirationSeconds must be greater than zero.")
            .ValidateOnStart();
        services.AddHttpClient<BcraClient>((serviceProvider, client) =>
        {
            var bcraOptions = serviceProvider.GetRequiredService<IOptions<BcraOptions>>().Value;
            client.BaseAddress = new Uri(bcraOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(bcraOptions.TimeoutSeconds);
        });

        services.AddTransient<CorrelationIdDelegatingHandler>();

        services.AddHttpClient<BcraClient>((serviceProvider, client) =>
        {
            var bcraOptions = serviceProvider.GetRequiredService<IOptions<BcraOptions>>().Value;
            client.BaseAddress = new Uri(bcraOptions.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(bcraOptions.TimeoutSeconds);
        })
        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

        services.AddScoped<IBcraClient>(serviceProvider => new CachedBcraClient(
            serviceProvider.GetRequiredService<BcraClient>(),
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<IOptions<CacheOptions>>(),
            serviceProvider.GetRequiredService<ILogger<CachedBcraClient>>()));

        return services;
    }
}