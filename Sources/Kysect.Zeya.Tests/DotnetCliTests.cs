using FluentAssertions;
using Kysect.CommonLib.DependencyInjection;
using Kysect.Zeya.ManagedDotnetCli;

namespace Kysect.Zeya.Tests;

public class DotnetCliTests
{
    [Test]
    public void GetProperty_IsTestProject_ReturnTrue()
    {
        var logger = PredefinedLogger.CreateConsoleLogger();
        var dotnetCli = new DotnetCli(logger);

        var isTestProject = dotnetCli.GetProperty(Path.Combine("..", "..", "..", "Kysect.Zeya.Tests.csproj"), "IsTestProject");

        var parsed = bool.TryParse(isTestProject, out var result);
        parsed.Should().BeTrue();
        result.Should().BeTrue();
    }

    [Test]
    public void GetProperty_TargetFramework_Return80()
    {
        var logger = PredefinedLogger.CreateConsoleLogger();
        var dotnetCli = new DotnetCli(logger);

        var isTestProject = dotnetCli.GetProperty(Path.Combine("..", "..", "..", "Kysect.Zeya.Tests.csproj"), "TargetFramework");

        isTestProject.Should().Be("net8.0");
    }
}