namespace Sery.Domain.Entities;

public sealed class User
{
    public Guid Id { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string PreferredLanguage { get; init; } = "es";

    public ICollection<Conversation> Conversations { get; init; } = [];
}
