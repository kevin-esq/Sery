using Sery.Domain.Entities;

namespace Sery.Application.Chat;

public static class AgentPersonaCustomizationMerger
{
    public static AgentPersonaProfile Merge(
        AgentPersonaProfile basePersona,
        UserAgentCustomization? customization)
    {
        if (customization is null)
        {
            return basePersona;
        }

        return new AgentPersonaProfile(
            basePersona.AgentName,
            customization.IdentityPresentation,
            customization.CoreDemeanor,
            customization.Warmth,
            customization.Directness,
            customization.Sincerity,
            customization.Charisma,
            customization.Playfulness,
            customization.Reflection,
            customization.Proactivity,
            customization.EmotionalExpressiveness,
            customization.PreferredResponseLength,
            customization.AskFollowUpQuestions,
            customization.OfferActionSteps,
            basePersona.PersonalityKeywords,
            new RelationalPersonaProfile(
                ParseRelationshipMode(customization.RelationshipMode),
                customization.Closeness,
                customization.Tenderness,
                customization.Protectiveness,
                customization.Flirtiness,
                customization.UsesAffectionateLanguage,
                customization.AllowsRomanticFraming,
                customization.PrioritizeSupportOverRoleplay),
            basePersona.SafetyProfile);
    }

    private static RelationshipMode ParseRelationshipMode(string value)
    {
        string normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "listener" => RelationshipMode.Listener,
            "friend" => RelationshipMode.Friend,
            "closecompanion" or "close_companion" or "close-companion" => RelationshipMode.CloseCompanion,
            "romanticcompanion" or "romantic_companion" or "romantic-companion" => RelationshipMode.RomanticCompanion,
            "supportcoach" or "support_coach" or "support-coach" => RelationshipMode.SupportCoach,
            _ => RelationshipMode.Friend
        };
    }
}
