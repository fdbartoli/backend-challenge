namespace Banco.Infrastructure.External.Bcra;

public class BcraOptions
{
    public const string SectionName = "Bcra";

    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; }
}