using Microsoft.Extensions.DependencyInjection;
using Sery.Application.Chat;

namespace Sery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IChatMessageService, ChatMessageService>();

        return services;
    }
}
