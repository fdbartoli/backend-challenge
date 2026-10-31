namespace Banco.Infrastructure.External.Bcra;

public class BcraOptions
{
    public const string SectionName = "Bcra";

    public string BaseUrl { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; }

    public BcraResilienceOptions Resilience { get; set; } = new();
}

public class BcraResilienceOptions
{
    public int RetryCount { get; set; }
    public int RetryBaseDelayMilliseconds { get; set; }

    public double CircuitBreakerFailureRatio { get; set; }
    public int CircuitBreakerMinimumThroughput { get; set; }
    public int CircuitBreakerSamplingDurationSeconds { get; set; }
    public int CircuitBreakerBreakDurationSeconds { get; set; }
}