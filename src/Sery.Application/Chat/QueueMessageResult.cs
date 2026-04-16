namespace Sery.Application.Chat;

public sealed record QueueMessageResult(Guid ConversationId, string AssistantMessage);
