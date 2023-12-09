using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.Zeya.ManagedDotnetCli;

namespace Kysect.Zeya.Tests;

public class DotnetCliTests
{
    [Test]
    public void GetProperty_IsTestProject_ReturnTrue()
    {
        var logger = DefaultLoggerConfiguration.CreateConsoleLogger();
        var dotnetCli = new DotnetCli(logger);

        var isTestProject = dotnetCli.GetProperty(Path.Combine("..", "..", "..", "Kysect.Zeya.Tests.csproj"), "IsTestProject");

        var parsed = bool.TryParse(isTestProject, out var result);
        parsed.Should().BeTrue();
        result.Should().BeTrue();
    }

    [Test]
    public void GetProperty_TargetFramework_Return80()
    {
        var logger = DefaultLoggerConfiguration.CreateConsoleLogger();
        var dotnetCli = new DotnetCli(logger);

        var targetFramework= dotnetCli.GetProperty(Path.Combine("..", "..", "..", "Kysect.Zeya.Tests.csproj"), "TargetFramework");

        targetFramework.Should().Be("net8.0");
    }
}