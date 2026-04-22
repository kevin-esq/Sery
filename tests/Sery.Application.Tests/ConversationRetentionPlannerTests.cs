using Sery.Application.Chat;

namespace Sery.Application.Tests;

public sealed class ConversationRetentionPlannerTests
{
    private static readonly AgentPersonaProfile Persona = new(
        "Sery",
        "neutral",
        "supportive",
        80,
        55,
        90,
        60,
        40,
        75,
        65,
        55,
        "medium",
        true,
        true,
        ["warm", "grounded"],
        new RelationalPersonaProfile(
            RelationshipMode.Friend,
            70,
            60,
            55,
            0,
            false,
            false,
            true),
        new CompanionSafetyProfile(
            true,
            true,
            true,
            true,
            true,
            true));

    private static readonly AgentAdaptationProfile Adaptation = new(
        true,
        true,
        true,
        true,
        true,
        65,
        30);

    [Fact]
    public void CreatePlan_ShouldSelectGroundingMode_ForAnxiousMessage()
    {
        var planner = new HeuristicConversationRetentionPlanner();
        var analysis = new TurnAnalysis(
            "anxious",
            EmotionalIntensity.High,
            EnergyLevel.High,
            FormalityLevel.Neutral,
            SelfDisclosureDepth.Medium,
            UserNeed.Regulation,
            false,
            false,
            false,
            true,
            false,
            true);
        var pacing = new AdaptivePacingPlan("short", "slow", "light", 1, false, false, "short", "soft", "gentle");

        RetentionPromptPlan plan = planner.CreatePlan(
            new ConversationContext(null, [], "es", null, null, null, [], []),
            "Me siento muy ansioso y nervioso hoy",
            analysis,
            pacing,
            Persona,
            Adaptation);

        Assert.Equal(ConversationMode.Grounding, plan.Mode);
    }

    [Fact]
    public void CreatePlan_ShouldSelectDirectMode_ForActionSeekingMessage()
    {
        var planner = new HeuristicConversationRetentionPlanner();
        var analysis = new TurnAnalysis(
            "mixed",
            EmotionalIntensity.Low,
            EnergyLevel.Medium,
            FormalityLevel.Casual,
            SelfDisclosureDepth.Low,
            UserNeed.Action,
            true,
            false,
            false,
            false,
            false,
            false);
        var pacing = new AdaptivePacingPlan("short", "brisk", "minimal", 1, false, true, "clear", "fast", "momentum");

        RetentionPromptPlan plan = planner.CreatePlan(
            new ConversationContext(null, [], "es", null, null, null, [], []),
            "Solo dime qué hago, ve al grano",
            analysis,
            pacing,
            Persona,
            Adaptation);

        Assert.Equal(ConversationMode.Direct, plan.Mode);
    }
}
