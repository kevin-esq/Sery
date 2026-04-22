namespace Sery.Domain.Entities;

public sealed class UserEmotionalMemory
{
    public Guid UserId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string DominantEmotion { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
