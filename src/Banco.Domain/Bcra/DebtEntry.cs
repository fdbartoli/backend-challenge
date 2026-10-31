namespace Banco.Domain.Bcra;

public sealed record DebtEntry(string Institution, int Situation, decimal Amount, int DaysOverdue);