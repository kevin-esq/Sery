namespace Sery.Application.Chat;

public sealed record ChatEfficacyProfileSnapshot(
    string PreferredConversationMode,
    string PreferredResponseLength,
    string PreferredQuestionStyle,
    string PreferredActionStyle,
    string PreferredPacing,
    string SuccessfulStrategiesSummary,
    DateTime UpdatedAt);
