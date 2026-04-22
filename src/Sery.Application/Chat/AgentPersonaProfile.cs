namespace Sery.Application.Chat;

public sealed record AgentPersonaProfile(
    string AgentName,
    string IdentityPresentation,
    string CoreDemeanor,
    int Warmth,
    int Directness,
    int Sincerity,
    int Charisma,
    int Playfulness,
    int Reflection,
    int Proactivity,
    int EmotionalExpressiveness,
    string PreferredResponseLength,
    bool AskFollowUpQuestions,
    bool OfferActionSteps,
    IReadOnlyList<string> PersonalityKeywords,
    RelationalPersonaProfile RelationalStyle,
    CompanionSafetyProfile SafetyProfile);
