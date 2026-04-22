namespace Sery.API.Contracts.AgentCustomization;

public sealed class AgentCustomizationResponse
{
    public string AgentName { get; init; } = string.Empty;
    public string IdentityPresentation { get; init; } = string.Empty;
    public string CoreDemeanor { get; init; } = string.Empty;
    public int Warmth { get; init; }
    public int Directness { get; init; }
    public int Sincerity { get; init; }
    public int Charisma { get; init; }
    public int Playfulness { get; init; }
    public int Reflection { get; init; }
    public int Proactivity { get; init; }
    public int EmotionalExpressiveness { get; init; }
    public string PreferredResponseLength { get; init; } = string.Empty;
    public bool AskFollowUpQuestions { get; init; }
    public bool OfferActionSteps { get; init; }
    public string RelationshipMode { get; init; } = string.Empty;
    public int Closeness { get; init; }
    public int Tenderness { get; init; }
    public int Protectiveness { get; init; }
    public int Flirtiness { get; init; }
    public bool UsesAffectionateLanguage { get; init; }
    public bool AllowsRomanticFraming { get; init; }
    public bool PrioritizeSupportOverRoleplay { get; init; }
    public bool IsCustomized { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
