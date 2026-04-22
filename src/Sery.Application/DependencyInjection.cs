using Microsoft.Extensions.DependencyInjection;
using Sery.Application.AgentCustomization;
using Sery.Application.Auth;
using Sery.Application.Chat;
using Sery.Application.Conversations;

namespace Sery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IChatMessageService, ChatMessageService>();
        services.AddScoped<ITurnAnalyzer, HeuristicTurnAnalyzer>();
        services.AddScoped<IMemoryOrchestrator, HeuristicMemoryOrchestrator>();
        services.AddScoped<IUserFactExtractor, HeuristicUserFactExtractor>();
        services.AddScoped<IResponsePolicyEngine, HeuristicResponsePolicyEngine>();
        services.AddScoped<IAdaptivePacingPolicy, HeuristicAdaptivePacingPolicy>();
        services.AddScoped<IChatEfficacyProfiler, HeuristicChatEfficacyProfiler>();
        services.AddScoped<IConversationRetentionPlanner, HeuristicConversationRetentionPlanner>();
        services.AddScoped<IHookEngine, HeuristicHookEngine>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAgentCustomizationService, AgentCustomizationService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IConversationInsightsService, ConversationInsightsService>();

        return services;
    }
}
