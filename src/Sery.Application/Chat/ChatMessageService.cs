using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Sery.Application.Common.Interfaces;
using Sery.Domain.Entities;

namespace Sery.Application.Chat;

public sealed class ChatMessageService(
    IChatPersistence chatPersistence,
    IChatAIService chatAIService,
    IUserContext userContext,
    ILogger<ChatMessageService> logger) : IChatMessageService
{
    private const int ConversationContextLimit = 20;
    private const string FallbackAssistantMessage =
        "I'm having trouble responding right now, but I'm here with you. Want to try again?";

    public async IAsyncEnumerable<StreamChunkDto> QueueMessageStreamAsync(
        QueueMessageCommand command,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        Guid userId = userContext.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");

        ConversationContext context = await chatPersistence.GetConversationContextAsync(
            userId, ConversationContextLimit, cancellationToken);

        Conversation conversation = context.Conversation ?? new Conversation { Id = Guid.NewGuid(), UserId = userId };
        if (context.Conversation is null)
        {
            chatPersistence.AddConversation(conversation);
        }

        var userMsg = new Message
        {
            ConversationId = conversation.Id,
            Role = MessageRole.User,
            Content = command.Message.Trim()
        };
        chatPersistence.AddMessage(userMsg);
        await chatPersistence.SaveChangesAsync(cancellationToken);

        string systemPrompt = $"You are Sery. Always respond in {context.Language}.";

        var aiMessages = new List<ChatMessageDto>
        {
            new("system", systemPrompt)
        };
        aiMessages.AddRange(context.History.Select(m => new ChatMessageDto(MapRole(m.Role), m.Content)));
        aiMessages.Add(new("user", userMsg.Content));

        var sb = new StringBuilder();

        IAsyncEnumerator<string> stream = chatAIService.GenerateStreamAsync(aiMessages, cancellationToken).GetAsyncEnumerator(cancellationToken);

        try
        {
            bool hasMore = true;
            while (hasMore)
            {
                string? token = null;
                try
                {
                    if (await stream.MoveNextAsync())
                    {
                        token = stream.Current;
                    }
                    else
                    {
                        hasMore = false;
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.LogWarning("Response stream canceled by the user.");
                    hasMore = false;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Critical error when contacting the AI service.");
                    token = FallbackAssistantMessage;
                    hasMore = false;
                }

                if (!string.IsNullOrEmpty(token))
                {
                    _ = sb.Append(token);
                    yield return new StreamChunkDto(token, false, conversation.Id);
                }
            }
        }
        finally
        {
            if (sb.Length > 0)
            {
                var assistantMsg = new Message
                {
                    ConversationId = conversation.Id,
                    Role = MessageRole.Assistant,
                    Content = sb.ToString()
                };
                chatPersistence.AddMessage(assistantMsg);
                await chatPersistence.SaveChangesAsync(CancellationToken.None);
            }
            await stream.DisposeAsync();
        }

        yield return new StreamChunkDto(string.Empty, true, conversation.Id);
    }

    private static string MapRole(MessageRole role)
    {
        return role switch
        {
            MessageRole.User => "user",
            MessageRole.Assistant => "assistant",
            _ => "user"
        };
    }
}
