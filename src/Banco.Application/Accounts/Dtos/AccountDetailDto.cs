namespace Banco.Application.Accounts.Dtos;

public record AccountDetailDto(
    int Id,
    int CustomerId,
    string Number,
    string BranchCode,
    decimal Balance,
    IReadOnlyList<TransactionDto> Transactions);