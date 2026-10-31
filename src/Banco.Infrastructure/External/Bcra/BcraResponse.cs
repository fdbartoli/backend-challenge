using System.Text.Json.Serialization;

namespace Banco.Infrastructure.External.Bcra;

internal sealed class BcraResponse
{
    [JsonPropertyName("results")]
    public BcraResults? Results { get; init; }
}

internal sealed class BcraResults
{
    [JsonPropertyName("identificacion")]
    public long Identification { get; init; }

    [JsonPropertyName("denominacion")]
    public string? Name { get; init; }

    [JsonPropertyName("periodos")]
    public List<BcraPeriod> Periods { get; init; } = new();
}

internal sealed class BcraPeriod
{
    [JsonPropertyName("periodo")]
    public string Period { get; init; } = string.Empty;

    [JsonPropertyName("entidades")]
    public List<BcraEntity> Entities { get; init; } = new();
}

internal sealed class BcraEntity
{
    [JsonPropertyName("entidad")]
    public string Institution { get; init; } = string.Empty;

    [JsonPropertyName("situacion")]
    public int Situation { get; init; }

    [JsonPropertyName("monto")]
    public decimal Amount { get; init; }

    [JsonPropertyName("diasAtrasoPago")]
    public int DaysOverdue { get; init; }
}