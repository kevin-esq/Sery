using Microsoft.Extensions.DependencyInjection;
using Sery.Application.Auth;
using Sery.Application.Chat;

namespace Sery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IChatMessageService, ChatMessageService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
