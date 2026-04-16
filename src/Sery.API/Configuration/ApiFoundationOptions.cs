namespace Sery.API.Configuration;

public sealed class ApiFoundationOptions
{
    public const string SectionName = "ApiBehavior";

    public string ServiceName { get; init; } = "Sery.API";
    public bool IncludeExceptionDetails { get; init; }
}
