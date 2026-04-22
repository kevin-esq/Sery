using Sery.Application.Chat;

namespace Sery.Application.Tests;

public class ChatEfficacyProfilerTests
{
    private readonly HeuristicChatEfficacyProfiler _profiler = new();

    [Fact]
    public void BuildProfileShouldPreferDeepModeForLongIntrospectiveHistory()
    {
        IReadOnlyList<string> recentMessages =
        [
            "Últimamente siento que estoy cargando demasiadas expectativas y eso me deja pensando durante horas en qué parte de mí está intentando sostenerlo todo.",
            "También noto que cuando me exijo demasiado me cierro y me cuesta explicar qué necesito realmente."
        ];

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

        var pacing = new AdaptivePacingPlan("medium-long", "slow", "light", 1, true, false, "layered", "soft", "reflective");

        ChatEfficacyProfileSnapshot profile = _profiler.BuildProfile(
            recentMessages,
            analysis,
            ConversationMode.Deep,
            pacing);

        Assert.Equal("Deep", profile.PreferredConversationMode);
        Assert.Equal("medium-long", profile.PreferredResponseLength);
    }
}
