namespace Banco.Application.Customers.Dtos;

public record CustomerDetailDto(
    int Id,
    string FirstName,
    string LastName,
    string Cuit,
    IReadOnlyList<AccountSummaryDto> Accounts,
    BcraSituationDto Bcra);