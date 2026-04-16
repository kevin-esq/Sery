namespace Sery.Application.Chat;

public sealed record QueueMessageResult(Guid UserId, string Message, string Status);
