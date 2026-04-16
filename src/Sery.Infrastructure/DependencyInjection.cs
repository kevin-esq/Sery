using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sery.Application.Chat;
using Sery.Infrastructure.AI;
using Sery.Infrastructure.Persistence;

namespace Sery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString =
            Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION")
            ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS_DEFAULTCONNECTION")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings_DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
            ?? configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");
        }

        services.AddDbContext<SeryDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IChatPersistence, EfChatPersistence>();
        services.Configure<ChatAIOptions>(configuration.GetSection(ChatAIOptions.SectionName));
        services.AddHttpClient<IChatAIService, OpenAIChatAIService>();

        return services;
    }
}
