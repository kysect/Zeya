using FluentAssertions;
using Kysect.CommonLib.DependencyInjection;
using Kysect.Zeya.ManagedDotnetCli;

namespace Kysect.Zeya.Tests;

public class DotnetCliTests
{
    [Test]
    public void GetProperty_ForManagePackageVersionsCentrally_ReturnFalse()
    {
        var logger = PredefinedLogger.CreateConsoleLogger();
        var dotnetCli = new DotnetCli(logger);

        var isTestProject = dotnetCli.GetProperty(Path.Combine("..", "..", "..", "Kysect.Zeya.Tests.csproj"), "IsTestProject");

        var parsed = bool.TryParse(isTestProject, out var result);
        parsed.Should().BeTrue();
        result.Should().BeTrue();
    }
}