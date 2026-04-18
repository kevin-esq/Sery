using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__RUNTIMECONNECTION")
            ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULTCONNECTION")
            ?? Environment.GetEnvironmentVariable("CONNECTIONSTRINGS_DEFAULTCONNECTION")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__RuntimeConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings_DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
            ?? FirstNonEmpty(
                configuration.GetConnectionString("RuntimeConnection"),
                configuration.GetConnectionString("DefaultConnection"));

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");
        }

        connectionString = NpgsqlSupabaseConnection.ApplyPoolerDefaults(connectionString);

        services.AddDbContext<SeryDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IChatPersistence, EfChatPersistence>();
        services.Configure<ChatAIOptions>(configuration.GetSection(ChatAIOptions.SectionName));

        services.AddHttpClient<OllamaChatAIService>(client => client.Timeout = TimeSpan.FromMinutes(3));
        services.AddHttpClient<OpenAIChatAIService>(client => client.Timeout = TimeSpan.FromMinutes(3));

        services.AddScoped<IChatAIService>(sp =>
        {
            ChatAIOptions options = sp.GetRequiredService<IOptions<ChatAIOptions>>().Value;

            return options.Provider.Trim().ToLowerInvariant() switch
            {
                "ollama" => sp.GetRequiredService<OllamaChatAIService>(),
                "openai" => sp.GetRequiredService<OpenAIChatAIService>(),
                _ => throw new InvalidOperationException($"Unknown ChatAI provider: '{options.Provider}'.")
            };
        });

        return services;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (string? v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
            {
                return v;
            }
        }

        return null;
    }
}
