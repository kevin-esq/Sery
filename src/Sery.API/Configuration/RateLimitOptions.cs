namespace Sery.API.Configuration;

public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    public int FreeTierDailyMessageLimit { get; init; } = 20;
}
