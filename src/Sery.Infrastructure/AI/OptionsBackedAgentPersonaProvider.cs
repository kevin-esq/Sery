using Microsoft.Extensions.Options;
using Sery.Application.Chat;

namespace Sery.Infrastructure.AI;

public sealed class OptionsBackedAgentPersonaProvider(IOptions<ChatAIOptions> options) : IAgentPersonaProvider
{
    private readonly ChatAIOptions _options = options.Value;

    public AgentPersonaProfile GetPersona()
    {
        AgentPersonaOptions persona = _options.AgentPersona;
        RelationalPersonaOptions relational = persona.RelationalStyle;
        CompanionSafetyOptions safety = persona.Safety;

        return new AgentPersonaProfile(
            persona.AgentName,
            persona.IdentityPresentation,
            persona.CoreDemeanor,
            persona.Warmth,
            persona.Directness,
            persona.Sincerity,
            persona.Charisma,
            persona.Playfulness,
            persona.Reflection,
            persona.Proactivity,
            persona.EmotionalExpressiveness,
            persona.PreferredResponseLength,
            persona.AskFollowUpQuestions,
            persona.OfferActionSteps,
            persona.PersonalityKeywords,
            new RelationalPersonaProfile(
                ParseRelationshipMode(relational.RelationshipMode),
                relational.Closeness,
                relational.Tenderness,
                relational.Protectiveness,
                relational.Flirtiness,
                relational.UsesAffectionateLanguage,
                relational.AllowsRomanticFraming,
                relational.PrioritizeSupportOverRoleplay),
            new CompanionSafetyProfile(
                safety.MustStayExplicitlyAI,
                safety.BlockExclusiveBondingLanguage,
                safety.EncourageOfflineSupport,
                safety.DeescalateDuringCrisis,
                safety.DisallowSexualContent,
                safety.DisallowManipulativeDependencyLanguage));
    }

    public AgentAdaptationProfile GetAdaptation()
    {
        AgentAdaptationOptions adaptation = _options.Adaptation;

        return new AgentAdaptationProfile(
            adaptation.Enabled,
            adaptation.MirrorUserEnergy,
            adaptation.MirrorUserFormality,
            adaptation.MirrorUserVerbosity,
            adaptation.AllowModeShifts,
            adaptation.AdaptationStrength,
            adaptation.MaxToneShiftPerTurn);
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
