using NetArchTest.Rules;
using Sery.API.Controllers.V1;

namespace Sery.Architecture.Tests;

/// <summary>
/// Ensures HTTP controllers stay under versioned folders/namespaces for regression isolation.
/// </summary>
public class ApiControllerOrganizationTests
{
    [Fact]
    public void ApiControllers_ShouldResideIn_VersionedNamespace()
    {
        TestResult? result = Types.InAssembly(typeof(ChatController).Assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .And()
            .AreClasses()
            .Should()
            .ResideInNamespaceMatching("^Sery\\.API\\.Controllers\\.V\\d+$")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result)
    {
        return result.IsSuccessful ? string.Empty : string.Join(Environment.NewLine, result.FailingTypes.Select(t => t.FullName));
    }
}
