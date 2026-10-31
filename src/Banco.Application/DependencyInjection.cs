using Banco.Application.Accounts;
using Banco.Application.Customers;
using Microsoft.Extensions.DependencyInjection;

namespace Banco.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IAccountService, AccountService>();

        return services;
    }
}