using Sery.Application.Chat;

namespace Sery.Application.Tests;

public class AdaptivePacingPolicyTests
{
    private readonly HeuristicAdaptivePacingPolicy _policy = new();
    private static readonly AgentPersonaProfile Persona = new(
        "Sery",
        "neutral",
        "warm and grounded",
        70,
        50,
        80,
        60,
        25,
        75,
        55,
        60,
        "medium",
        true,
        true,
        ["clear", "empathetic"],
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

    private static readonly AgentAdaptationProfile Adaptation = new(true, true, true, true, true, 70, 40);

    [Fact]
    public void CreatePlanShouldUseLongerStructuredOutputForReflectiveHighDisclosureTurn()
    {
        var context = new ConversationContext(null, [], "es", null, null, null, [], []);
        var analysis = new TurnAnalysis(
            "sad",
            EmotionalIntensity.Moderate,
            EnergyLevel.Low,
            FormalityLevel.Neutral,
            SelfDisclosureDepth.High,
            UserNeed.Reflection,
            false,
            true,
            true,
            true,
            false,
            false);

        AdaptivePacingPlan plan = _policy.CreatePlan(
            context,
            "Quiero entender por qué me está costando tanto sostener todo esto y qué hay detrás.",
            analysis,
            Persona,
            Adaptation);

        Assert.True(plan.UseParagraphBreaks);
        Assert.Equal("medium-long", plan.PreferredResponseLength);
    }

    [Fact]
    public void CreatePlanShouldKeepResponseShortForRegulationTurn()
    {
        var context = new ConversationContext(null, [], "es", null, null, null, [], []);
        var analysis = new TurnAnalysis(
            "anxious",
            EmotionalIntensity.High,
            EnergyLevel.High,
            FormalityLevel.Casual,
            SelfDisclosureDepth.Medium,
            UserNeed.Regulation,
            false,
            false,
            false,
            true,
            false,
            true);

        AdaptivePacingPlan plan = _policy.CreatePlan(
            context,
            "Estoy muy ansioso, dime algo simple por favor.",
            analysis,
            Persona,
            Adaptation);

        Assert.Equal("short", plan.PreferredResponseLength);
        Assert.True(plan.QuestionBudget <= 1);
    }
}
