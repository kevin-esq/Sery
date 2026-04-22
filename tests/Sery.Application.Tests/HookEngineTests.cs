using Sery.Application.Chat;
using Sery.Domain.Entities;

namespace Sery.Application.Tests;

public sealed class HookEngineTests
{
    [Fact]
    public void BuildHooks_ShouldIncludeContinuityHook_WhenConversationHasSummary()
    {
        var engine = new HeuristicHookEngine();
        var persona = new AgentPersonaProfile(
            "Sery",
            "neutral",
            "supportive",
            80,
            55,
            90,
            70,
            20,
            75,
            65,
            55,
            "medium",
            true,
            true,
            ["warm"],
            new RelationalPersonaProfile(
                RelationshipMode.CloseCompanion,
                80,
                75,
                60,
                10,
                true,
                false,
                true),
            new CompanionSafetyProfile(
                true,
                true,
                true,
                true,
                true,
                true));
        var adaptation = new AgentAdaptationProfile(true, true, true, true, true, 65, 30);
        var conversation = new Conversation
        {
            Summary = "The conversation focuses on work pressure."
        };
        var analysis = new TurnAnalysis(
            "confused",
            EmotionalIntensity.Moderate,
            EnergyLevel.Medium,
            FormalityLevel.Neutral,
            SelfDisclosureDepth.High,
            UserNeed.Reflection,
            false,
            true,
            true,
            true,
            false,
            false);
        var pacing = new AdaptivePacingPlan("medium-long", "measured", "visible", 1, true, false, "layered", "soft", "reflective");

        IReadOnlyList<RetentionHook> hooks = engine.BuildHooks(
            new ConversationContext(conversation, [], "es", null, null, null, [], []),
            "No sé cómo hablar con mi jefe",
            analysis,
            ConversationMode.Reflective,
            pacing,
            persona,
            adaptation);

        Assert.Contains(hooks, x => x.Name == "continuity-callback");
        Assert.Contains(hooks, x => x.Name == "inner-contrast");
    }
}
