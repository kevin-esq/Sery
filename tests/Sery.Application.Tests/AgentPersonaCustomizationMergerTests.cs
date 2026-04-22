using Sery.Application.Chat;
using Sery.Domain.Entities;

namespace Sery.Application.Tests;

public class AgentPersonaCustomizationMergerTests
{
    [Fact]
    public void Merge_ShouldOverrideBasePersona_WhenCustomizationExists()
    {
        var basePersona = new AgentPersonaProfile(
            "Sery",
            "neutral",
            "supportive",
            70,
            50,
            80,
            60,
            20,
            70,
            60,
            50,
            "medium",
            true,
            true,
            ["warm"],
            new RelationalPersonaProfile(RelationshipMode.Friend, 70, 60, 50, 0, false, false, true),
            new CompanionSafetyProfile(true, true, true, true, true, true));

        var customization = new UserAgentCustomization
        {
            UserId = Guid.NewGuid(),
            IdentityPresentation = "feminine",
            Warmth = 90,
            RelationshipMode = "romantic-companion",
            Flirtiness = 40,
            UsesAffectionateLanguage = true,
            AllowsRomanticFraming = true
        };

        AgentPersonaProfile merged = AgentPersonaCustomizationMerger.Merge(basePersona, customization);

        Assert.Equal("feminine", merged.IdentityPresentation);
        Assert.Equal(90, merged.Warmth);
        Assert.Equal(RelationshipMode.RomanticCompanion, merged.RelationalStyle.RelationshipMode);
        Assert.True(merged.RelationalStyle.UsesAffectionateLanguage);
        Assert.True(merged.RelationalStyle.AllowsRomanticFraming);
        Assert.Equal(basePersona.SafetyProfile, merged.SafetyProfile);
    }
}
