namespace Banco.Application.Customers.Dtos;

public record BcraSituationDto(
    string Status,
    string? Period,
    int? WorstSituation,
    decimal? TotalAmount,
    IReadOnlyList<BcraEntityDto> Entities);