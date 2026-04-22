using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sery.Application.AgentCustomization;
using Sery.Application.Auth;
using Sery.Application.Chat;
using Sery.Application.Common.Interfaces;
using Sery.Application.Conversations;
using Sery.Infrastructure.AI;
using Sery.Infrastructure.AI.Gemini;
using Sery.Infrastructure.AI.Ollama;
using Sery.Infrastructure.Authentication;
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

        _ = services.AddDbContext<SeryDbContext>(options =>
            options.UseNpgsql(connectionString));
        _ = services.AddScoped<IAuthPersistence, EfAuthPersistence>();
        _ = services.AddScoped<IAgentCustomizationPersistence, EfAgentCustomizationPersistence>();
        _ = services.AddScoped<IChatPersistence, EfChatPersistence>();
        _ = services.AddScoped<IConversationPersistence, EfConversationPersistence>();
        _ = services.Configure<ChatAIOptions>(configuration.GetSection(ChatAIOptions.SectionName));

        _ = services.AddHttpClient<OllamaChatAIService>(client => client.Timeout = TimeSpan.FromMinutes(3));
        _ = services.AddHttpClient<OpenAIChatAIService>(client => client.Timeout = TimeSpan.FromMinutes(3));
        _ = services.AddHttpClient<GeminiChatAIService>(client => client.Timeout = TimeSpan.FromMinutes(3));

        _ = services.AddHttpContextAccessor();
        _ = services.AddScoped<IUserContext, UserContext>();
        _ = services.AddScoped<IJwtService, JwtService>();
        _ = services.AddScoped<IPasswordHasher, PasswordHasher>();
        _ = services.AddScoped<IAgentPersonaProvider, OptionsBackedAgentPersonaProvider>();
        _ = services.AddScoped<ITurnStateInterpreter, AIBackedTurnStateInterpreter>();
        _ = services.AddScoped<IResponseCritic, AIBackedResponseCritic>();
        _ = services.AddScoped<IChatPromptComposer, DefaultChatPromptComposer>();

        _ = services.AddScoped<IChatAIService>(sp =>
        {
            ChatAIOptions options = sp.GetRequiredService<IOptions<ChatAIOptions>>().Value;

            IChatAIService innerService = options.Provider.Trim().ToLowerInvariant() switch
            {
                "ollama" => sp.GetRequiredService<OllamaChatAIService>(),
                "openai" => sp.GetRequiredService<OpenAIChatAIService>(),
                "gemini" => sp.GetRequiredService<GeminiChatAIService>(),
                _ => throw new InvalidOperationException($"Unknown ChatAI provider: '{options.Provider}'.")
            };

            return new ModeratedChatAIService(
                innerService,
                sp.GetRequiredService<IOptions<ChatAIOptions>>(),
                sp.GetRequiredService<ILogger<ModeratedChatAIService>>());
        });

        return services;
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
    }
}
