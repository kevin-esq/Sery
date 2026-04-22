namespace Sery.Domain.Entities;

public sealed class UserChatEfficacyProfile
{
    public Guid UserId { get; set; }
    public string PreferredConversationMode { get; set; } = string.Empty;
    public string PreferredResponseLength { get; set; } = string.Empty;
    public string PreferredQuestionStyle { get; set; } = string.Empty;
    public string PreferredActionStyle { get; set; } = string.Empty;
    public string PreferredPacing { get; set; } = string.Empty;
    public string SuccessfulStrategiesSummary { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
