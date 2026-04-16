using Microsoft.Extensions.Logging;
using Sery.Domain.Entities;

namespace Sery.Application.Chat;

public sealed class ChatMessageService(
    IChatPersistence chatPersistence,
    IChatAIService chatAIService,
    ILogger<ChatMessageService> logger) : IChatMessageService
{
    private const int ConversationContextLimit = 20;
    private const string FallbackAssistantMessage =
        "I'm having trouble responding right now, but I'm here with you. Want to try again?";

    public async Task<QueueMessageResult> QueueMessageAsync(
        QueueMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        string sanitizedMessage = command.Message.Trim();

        User? user = await chatPersistence.GetUserAsync(command.UserId, cancellationToken);
        if (user is null)
        {
            user = new User { Id = command.UserId };
            chatPersistence.AddUser(user);
        }

        Conversation? conversation = await chatPersistence.GetLatestConversationAsync(command.UserId, cancellationToken);
        if (conversation is null)
        {
            conversation = new Conversation
            {
                UserId = command.UserId
            };

            chatPersistence.AddConversation(conversation);
        }

        var message = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = sanitizedMessage
        };

        chatPersistence.AddMessage(message);
        await chatPersistence.SaveChangesAsync(cancellationToken);

        IReadOnlyList<Message> recentMessages = await chatPersistence.GetRecentMessagesAsync(
            conversation.Id,
            ConversationContextLimit,
            cancellationToken);

        List<ChatMessageDto> aiMessages = recentMessages
            .OrderBy(x => x.CreatedAt)
            .Select(x => new ChatMessageDto(MapRole(x.Role), x.Content))
            .ToList();

        string assistantContent;
        try
        {
            assistantContent = await chatAIService.GenerateResponseAsync(aiMessages, cancellationToken);
            if (string.IsNullOrWhiteSpace(assistantContent))
            {
                assistantContent = FallbackAssistantMessage;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate assistant response for conversation {ConversationId}", conversation.Id);
            assistantContent = FallbackAssistantMessage;
        }

        var assistantMessage = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.Assistant,
            Content = assistantContent.Trim()
        };

        chatPersistence.AddMessage(assistantMessage);
        await chatPersistence.SaveChangesAsync(cancellationToken);

        return new QueueMessageResult(conversation.Id, assistantMessage.Content);
    }

    private static string MapRole(MessageRole role) => role switch
    {
        MessageRole.User => "user",
        MessageRole.Assistant => "assistant",
        _ => "user"
    };
}
