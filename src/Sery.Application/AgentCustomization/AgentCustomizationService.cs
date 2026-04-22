using Sery.Application.Chat;
using Sery.Domain.Entities;

namespace Sery.Application.AgentCustomization;

public sealed class AgentCustomizationService(
    IAgentCustomizationPersistence persistence,
    IAgentPersonaProvider agentPersonaProvider) : IAgentCustomizationService
{
    public async Task<AgentCustomizationDto> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        UserAgentCustomization? customization = await persistence.GetAsync(userId, cancellationToken);
        AgentPersonaProfile effective = AgentPersonaCustomizationMerger.Merge(
            agentPersonaProvider.GetPersona(),
            customization);

        return ToDto(effective, customization);
    }

    public async Task<AgentCustomizationDto> UpdateAsync(
        Guid userId,
        UpdateAgentCustomizationCommand command,
        CancellationToken cancellationToken)
    {
        UserAgentCustomization? customization = await persistence.GetAsync(userId, cancellationToken);
        bool isNew = customization is null;
        customization ??= CreateDefaultCustomization(userId, agentPersonaProvider.GetPersona());

        Apply(customization, command);
        customization.UpdatedAt = DateTime.UtcNow;

        if (isNew)
        {
            persistence.Add(customization);
        }

        await persistence.SaveChangesAsync(cancellationToken);

        AgentPersonaProfile effective = AgentPersonaCustomizationMerger.Merge(
            agentPersonaProvider.GetPersona(),
            customization);

        return ToDto(effective, customization);
    }

    public Task ResetAsync(Guid userId, CancellationToken cancellationToken)
    {
        return persistence.DeleteAsync(userId, cancellationToken);
    }

    private static UserAgentCustomization CreateDefaultCustomization(Guid userId, AgentPersonaProfile persona)
    {
        return new UserAgentCustomization
        {
            UserId = userId,
            IdentityPresentation = persona.IdentityPresentation,
            CoreDemeanor = persona.CoreDemeanor,
            Warmth = persona.Warmth,
            Directness = persona.Directness,
            Sincerity = persona.Sincerity,
            Charisma = persona.Charisma,
            Playfulness = persona.Playfulness,
            Reflection = persona.Reflection,
            Proactivity = persona.Proactivity,
            EmotionalExpressiveness = persona.EmotionalExpressiveness,
            PreferredResponseLength = persona.PreferredResponseLength,
            AskFollowUpQuestions = persona.AskFollowUpQuestions,
            OfferActionSteps = persona.OfferActionSteps,
            RelationshipMode = persona.RelationalStyle.RelationshipMode.ToString(),
            Closeness = persona.RelationalStyle.Closeness,
            Tenderness = persona.RelationalStyle.Tenderness,
            Protectiveness = persona.RelationalStyle.Protectiveness,
            Flirtiness = persona.RelationalStyle.Flirtiness,
            UsesAffectionateLanguage = persona.RelationalStyle.UsesAffectionateLanguage,
            AllowsRomanticFraming = persona.RelationalStyle.AllowsRomanticFraming,
            PrioritizeSupportOverRoleplay = persona.RelationalStyle.PrioritizeSupportOverRoleplay
        };
    }

    private static void Apply(UserAgentCustomization customization, UpdateAgentCustomizationCommand command)
    {
        if (command.IdentityPresentation is not null) customization.IdentityPresentation = command.IdentityPresentation.Trim();
        if (command.CoreDemeanor is not null) customization.CoreDemeanor = command.CoreDemeanor.Trim();
        if (command.Warmth.HasValue) customization.Warmth = command.Warmth.Value;
        if (command.Directness.HasValue) customization.Directness = command.Directness.Value;
        if (command.Sincerity.HasValue) customization.Sincerity = command.Sincerity.Value;
        if (command.Charisma.HasValue) customization.Charisma = command.Charisma.Value;
        if (command.Playfulness.HasValue) customization.Playfulness = command.Playfulness.Value;
        if (command.Reflection.HasValue) customization.Reflection = command.Reflection.Value;
        if (command.Proactivity.HasValue) customization.Proactivity = command.Proactivity.Value;
        if (command.EmotionalExpressiveness.HasValue) customization.EmotionalExpressiveness = command.EmotionalExpressiveness.Value;
        if (command.PreferredResponseLength is not null) customization.PreferredResponseLength = command.PreferredResponseLength.Trim();
        if (command.AskFollowUpQuestions.HasValue) customization.AskFollowUpQuestions = command.AskFollowUpQuestions.Value;
        if (command.OfferActionSteps.HasValue) customization.OfferActionSteps = command.OfferActionSteps.Value;
        if (command.RelationshipMode is not null) customization.RelationshipMode = command.RelationshipMode.Trim();
        if (command.Closeness.HasValue) customization.Closeness = command.Closeness.Value;
        if (command.Tenderness.HasValue) customization.Tenderness = command.Tenderness.Value;
        if (command.Protectiveness.HasValue) customization.Protectiveness = command.Protectiveness.Value;
        if (command.Flirtiness.HasValue) customization.Flirtiness = command.Flirtiness.Value;
        if (command.UsesAffectionateLanguage.HasValue) customization.UsesAffectionateLanguage = command.UsesAffectionateLanguage.Value;
        if (command.AllowsRomanticFraming.HasValue) customization.AllowsRomanticFraming = command.AllowsRomanticFraming.Value;
        if (command.PrioritizeSupportOverRoleplay.HasValue) customization.PrioritizeSupportOverRoleplay = command.PrioritizeSupportOverRoleplay.Value;
    }

    private static AgentCustomizationDto ToDto(AgentPersonaProfile effective, UserAgentCustomization? customization)
    {
        return new AgentCustomizationDto(
            effective.AgentName,
            effective.IdentityPresentation,
            effective.CoreDemeanor,
            effective.Warmth,
            effective.Directness,
            effective.Sincerity,
            effective.Charisma,
            effective.Playfulness,
            effective.Reflection,
            effective.Proactivity,
            effective.EmotionalExpressiveness,
            effective.PreferredResponseLength,
            effective.AskFollowUpQuestions,
            effective.OfferActionSteps,
            effective.RelationalStyle.RelationshipMode.ToString(),
            effective.RelationalStyle.Closeness,
            effective.RelationalStyle.Tenderness,
            effective.RelationalStyle.Protectiveness,
            effective.RelationalStyle.Flirtiness,
            effective.RelationalStyle.UsesAffectionateLanguage,
            effective.RelationalStyle.AllowsRomanticFraming,
            effective.RelationalStyle.PrioritizeSupportOverRoleplay,
            customization is not null,
            customization?.UpdatedAt);
    }
}
