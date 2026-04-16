namespace Sery.Domain.Entities;

public sealed class Conversation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public User? User { get; init; }
    public ICollection<Message> Messages { get; init; } = new List<Message>();
}
