using Banco.Domain.Entities;
using Banco.Domain.ValueObjects;

namespace Banco.Application.Interfaces.Persistence;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken);

    Task<Customer?> GetByCuitWithAccountsAsync(Cuit cuit, CancellationToken cancellationToken);
}