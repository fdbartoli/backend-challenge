namespace Banco.Application.Customers.Dtos;

public record AccountSummaryDto(int Id, string Number, string BranchCode, decimal Balance);