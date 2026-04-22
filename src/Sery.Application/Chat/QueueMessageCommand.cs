namespace Sery.Application.Chat;

public sealed record QueueMessageCommand(string Message, Guid? ConversationId = null);
