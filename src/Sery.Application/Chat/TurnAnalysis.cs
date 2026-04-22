namespace Sery.Application.Chat;

public sealed record TurnAnalysis(
    string PrimaryEmotion,
    EmotionalIntensity EmotionalIntensity,
    EnergyLevel EnergyLevel,
    FormalityLevel FormalityLevel,
    SelfDisclosureDepth SelfDisclosureDepth,
    UserNeed UserNeed,
    bool WantsDirectAdvice,
    bool WantsReflection,
    bool IsQuestionHeavy,
    bool ShowsVulnerability,
    bool ShowsHumor,
    bool ContainsUrgencySignals);
