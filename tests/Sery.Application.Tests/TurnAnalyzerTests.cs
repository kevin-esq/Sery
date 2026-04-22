using Sery.Application.Chat;

namespace Sery.Application.Tests;

public class TurnAnalyzerTests
{
    private readonly HeuristicTurnAnalyzer _analyzer = new();

    [Fact]
    public void AnalyzeShouldDetectRegulationNeedForAnxiousMessage()
    {
        var context = new ConversationContext(null, [], "es", null, null, null, [], []);

        TurnAnalysis result = _analyzer.Analyze(context, "Me siento muy ansioso y abrumado, no sé cómo calmarme ahora mismo.");

        Assert.Equal(UserNeed.Regulation, result.UserNeed);
        Assert.True(result.EmotionalIntensity is EmotionalIntensity.Moderate or EmotionalIntensity.High);
        Assert.Equal("anxious", result.PrimaryEmotion);
        Assert.True(result.ShowsVulnerability);
    }

    [Fact]
    public void AnalyzeShouldDetectActionNeedForDirectRequest()
    {
        var context = new ConversationContext(null, [], "es", null, null, null, [], []);

        TurnAnalysis result = _analyzer.Analyze(context, "Dime exactamente qué pasos debo seguir para resolver esto hoy.");

        Assert.Equal(UserNeed.Action, result.UserNeed);
        Assert.True(result.WantsDirectAdvice);
    }
}
