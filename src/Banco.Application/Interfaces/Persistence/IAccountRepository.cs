using Banco.Domain.Entities;

namespace Banco.Application.Interfaces.Persistence;

public interface IAccountRepository
{
    Task<Account?> GetByIdWithTransactionsAsync(int id, CancellationToken cancellationToken);
}