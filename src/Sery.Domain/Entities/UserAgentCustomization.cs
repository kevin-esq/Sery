namespace Sery.Domain.Entities;

public sealed class UserAgentCustomization
{
    public Guid UserId { get; set; }
    public string IdentityPresentation { get; set; } = "neutral";
    public string CoreDemeanor { get; set; } = "supportive";
    public int Warmth { get; set; } = 75;
    public int Directness { get; set; } = 55;
    public int Sincerity { get; set; } = 85;
    public int Charisma { get; set; } = 60;
    public int Playfulness { get; set; } = 20;
    public int Reflection { get; set; } = 70;
    public int Proactivity { get; set; } = 65;
    public int EmotionalExpressiveness { get; set; } = 55;
    public string PreferredResponseLength { get; set; } = "medium";
    public bool AskFollowUpQuestions { get; set; } = true;
    public bool OfferActionSteps { get; set; } = true;
    public string RelationshipMode { get; set; } = "friend";
    public int Closeness { get; set; } = 70;
    public int Tenderness { get; set; } = 65;
    public int Protectiveness { get; set; } = 55;
    public int Flirtiness { get; set; } = 0;
    public bool UsesAffectionateLanguage { get; set; }
    public bool AllowsRomanticFraming { get; set; }
    public bool PrioritizeSupportOverRoleplay { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
