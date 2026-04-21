namespace Sery.Domain.Entities;

public sealed class User
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid TenantId { get; init; } = Guid.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string PreferredLanguage { get; init; } = "es";

    public ICollection<UserSession> Sessions { get; init; } = [];
    public ICollection<Conversation> Conversations { get; init; } = [];
}
