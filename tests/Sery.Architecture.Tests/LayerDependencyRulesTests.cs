using NetArchTest.Rules;
using Sery.Application.Chat;
using Sery.Domain.Entities;
using Sery.Infrastructure;

namespace Sery.Architecture.Tests;

public class LayerDependencyRulesTests
{
    [Fact]
    public void Domain_ShouldNotDependOn_Application()
    {
        TestResult? result = Types.InAssembly(typeof(UserProfile).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Sery.Application")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        TestResult? result = Types.InAssembly(typeof(UserProfile).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Sery.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Domain_ShouldNotDependOn_API()
    {
        TestResult? result = Types.InAssembly(typeof(UserProfile).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Sery.API")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Application_ShouldNotDependOn_Infrastructure()
    {
        TestResult? result = Types.InAssembly(typeof(ChatMessageService).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Sery.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Application_ShouldNotDependOn_API()
    {
        TestResult? result = Types.InAssembly(typeof(ChatMessageService).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Sery.API")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_API()
    {
        TestResult? result = Types.InAssembly(typeof(DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Sery.API")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result)
    {
        if (result.IsSuccessful)
        {
            return string.Empty;
        }

        return string.Join(Environment.NewLine, result.FailingTypes.Select(t => t.FullName));
    }
}
