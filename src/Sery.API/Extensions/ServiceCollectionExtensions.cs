using Sery.API.Configuration;

namespace Sery.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiFoundation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<ApiFoundationOptions>()
            .Bind(configuration.GetSection(ApiFoundationOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ServiceName),
                "ApiBehavior:ServiceName is required.")
            .ValidateOnStart();

        services
            .AddOptions<RateLimitOptions>()
            .Bind(configuration.GetSection(RateLimitOptions.SectionName))
            .Validate(options => options.FreeTierDailyMessageLimit > 0,
                "RateLimit:FreeTierDailyMessageLimit must be greater than zero.")
            .ValidateOnStart();

        services.AddProblemDetails();

        return services;
    }
}
