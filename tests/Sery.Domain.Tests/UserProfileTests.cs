using Sery.Domain.Entities;

namespace Sery.Domain.Tests;

public class UserProfileTests
{
    [Fact]
    public void UpdateDisplayName_ShouldTrimAndAssignValue_WhenInputContainsExtraSpaces()
    {
        var user = new UserProfile();

        user.UpdateDisplayName("  Asyama  ");

        Assert.Equal("Asyama", user.DisplayName);
    }

    [Fact]
    public void UpdateDisplayName_ShouldThrow_WhenValueIsEmpty()
    {
        var user = new UserProfile();

        Assert.Throws<ArgumentException>(() => user.UpdateDisplayName(" "));
    }
}
