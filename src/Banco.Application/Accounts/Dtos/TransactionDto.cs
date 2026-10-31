namespace Banco.Application.Accounts.Dtos;

public record TransactionDto(
    int Id,
    DateTime Date,
    string Type,
    string? Description,
    decimal Amount,
    decimal SignedAmount);