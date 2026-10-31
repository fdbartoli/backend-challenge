using Banco.Application.Accounts.Dtos;
using Banco.Application.Common.Exceptions;
using Banco.Application.Interfaces.Persistence;
using Banco.Domain.Entities;

namespace Banco.Application.Accounts;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<AccountDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdWithTransactionsAsync(id, cancellationToken);

        if (account is null)
        {
            throw new NotFoundException($"Account {id} was not found.");
        }

        return ToDetailDto(account);
    }

    private static AccountDetailDto ToDetailDto(Account account)
    {
        var transactions = account.Transactions
            .Select(t => new TransactionDto(t.Id, t.Date, t.Type, t.Description, t.Amount, t.SignedAmount))
            .ToList();

        return new AccountDetailDto(
            account.Id,
            account.CustomerId,
            account.Number,
            account.BranchCode,
            account.Balance,
            transactions);
    }
}