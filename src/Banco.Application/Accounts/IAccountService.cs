using Banco.Application.Accounts.Dtos;

namespace Banco.Application.Accounts;

public interface IAccountService
{
    Task<AccountDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken);
}