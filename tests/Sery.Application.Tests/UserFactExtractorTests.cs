using Sery.Application.Chat;

namespace Sery.Application.Tests;

public class UserFactExtractorTests
{
    [Fact]
    public void ExtractShouldCaptureIdentityPreferenceAndResponsePreference()
    {
        var extractor = new HeuristicUserFactExtractor();
        var context = new ConversationContext(null, [], "es", null, null, null, [], []);
        var analysis = new TurnAnalysis(
            "calm",
            EmotionalIntensity.Low,
            EnergyLevel.Medium,
            FormalityLevel.Neutral,
            SelfDisclosureDepth.Medium,
            UserNeed.Clarity,
            false,
            false,
            false,
            false,
            false,
            false);

        IReadOnlyList<UserFactExtraction> facts = extractor.Extract(
            context,
            "Me llamo Juan, mi lenguaje favorito es C# y háblame directo.",
            analysis);

        Assert.Contains(facts, x => x.NormalizedKey == "identity:name:juan");
        Assert.Contains(facts, x => x.NormalizedKey == "preference:c");
        Assert.Contains(facts, x => x.NormalizedKey == "response-preference:direct");
    }
}
