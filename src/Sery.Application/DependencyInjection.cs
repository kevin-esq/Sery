using Microsoft.Extensions.DependencyInjection;

namespace Sery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
