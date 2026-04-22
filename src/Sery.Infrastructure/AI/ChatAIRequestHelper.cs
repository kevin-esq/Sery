using Sery.Application.Chat;

namespace Sery.Infrastructure.AI;

internal static class ChatAIRequestHelper
{
    public static string ResolveApiKey(ChatAIOptions options, string providerName)
    {
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return options.ApiKey;
        }

        string? environmentApiKey = providerName.ToLowerInvariant() switch
        {
            "gemini" => Environment.GetEnvironmentVariable("GEMINI_API_KEY"),
            "openai" => Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
            _ => Environment.GetEnvironmentVariable("CHAT_AI_API_KEY")
        };

        if (!string.IsNullOrWhiteSpace(environmentApiKey))
        {
            return environmentApiKey;
        }

        throw new InvalidOperationException($"ChatAI ApiKey is required for provider '{providerName}'.");
    }

    public static IReadOnlyList<ChatMessageDto> BuildProviderMessages(IReadOnlyList<ChatMessageDto> messages)
    {
        if (messages.Count == 0)
        {
            return [new ChatMessageDto("system", "You are a helpful assistant.")];
        }

        var requestMessages = messages
            .Select(x => new ChatMessageDto(x.Role, x.Content))
            .ToList();

        if (requestMessages.All(x => x.Role != "system"))
        {
            requestMessages.Insert(0, new ChatMessageDto("system", "You are a helpful assistant."));
        }

        return requestMessages;
    }

    public static string? ExtractSystemInstruction(IReadOnlyList<ChatMessageDto> messages)
    {
        return messages.FirstOrDefault(x => x.Role == "system")?.Content;
    }

    public static IReadOnlyList<ChatMessageDto> BuildConversationMessagesWithoutSystem(IReadOnlyList<ChatMessageDto> messages)
    {
        return messages.Where(x => x.Role != "system").ToList();
    }
}
