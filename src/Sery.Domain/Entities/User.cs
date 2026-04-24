namespace Sery.Domain.Entities;

public sealed class User
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid TenantId { get; init; } = Guid.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string PreferredLanguage { get; init; } = "es";
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    public ICollection<UserSession> Sessions { get; init; } = [];
    public ICollection<Conversation> Conversations { get; init; } = [];
    public ICollection<UserMemoryFact> MemoryFacts { get; init; } = [];
    public UserEmotionalMemory? EmotionalMemory { get; set; }
    public UserChatEfficacyProfile? ChatEfficacyProfile { get; set; }
    public UserAgentCustomization? AgentCustomization { get; set; }
}
