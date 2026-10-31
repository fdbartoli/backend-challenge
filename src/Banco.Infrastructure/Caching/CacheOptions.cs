namespace Banco.Infrastructure.Caching;

public class CacheOptions
{
    public const string SectionName = "Cache";

    public int BcraExpirationSeconds { get; set; }
}