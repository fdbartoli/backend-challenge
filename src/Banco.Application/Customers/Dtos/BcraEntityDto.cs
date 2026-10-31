namespace Banco.Application.Customers.Dtos;

public record BcraEntityDto(string Institution, int Situation, decimal Amount, int DaysOverdue);