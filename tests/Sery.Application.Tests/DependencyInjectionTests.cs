using Microsoft.Extensions.DependencyInjection;
using Sery.Application;

namespace Sery.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ShouldReturnServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        Assert.Same(services, result);
    }
}
