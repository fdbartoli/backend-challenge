using Banco.Application.Customers.Dtos;

namespace Banco.Application.Customers;

public interface ICustomerService
{
    Task<IReadOnlyList<CustomerSummaryDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<CustomerDetailDto> GetByCuitAsync(string cuit, CancellationToken cancellationToken);
}