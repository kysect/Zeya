using FluentAssertions;
using Kysect.CommonLib.DependencyInjection.Logging;
using Kysect.Zeya.ManagedDotnetCli;
using System.IO.Abstractions;
using Kysect.PowerShellRunner.Abstractions.Accessors;
using Kysect.PowerShellRunner.Accessors;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tests;

public class DotnetCliTests
{
    private readonly IFileSystem _fileSystem = new FileSystem();
    private readonly IPowerShellAccessor _powerShellAccessor;
    private readonly DotnetCli _dotnetCli;
    private readonly ILogger _logger;

    public DotnetCliTests()
    {
        _logger = DefaultLoggerConfiguration.CreateConsoleLogger();
        var powerShellAccessorFactory = new PowerShellAccessorFactory();
        _powerShellAccessor = new PowerShellAccessorDecoratorBuilder(powerShellAccessorFactory)
            .WithLogging(_logger)
            .Build();

        _dotnetCli = new DotnetCli(_logger, _powerShellAccessor);
    }

    [Test]
    public void GetProperty_IsTestProject_ReturnTrue()
    {
        var isTestProject = _dotnetCli.GetProperty(_fileSystem.Path.Combine("..", "..", "..", "Kysect.Zeya.Tests.csproj"), "IsTestProject");

        var parsed = bool.TryParse(isTestProject, out var result);
        parsed.Should().BeTrue();
        result.Should().BeTrue();
    }

    [Test]
    public void GetProperty_TargetFramework_Return80()
    {
        var targetFramework = _dotnetCli.GetProperty(_fileSystem.Path.Combine("..", "..", "..", "Kysect.Zeya.Tests.csproj"), "TargetFramework");

        targetFramework.Should().Be("net8.0");
    }

    [OneTimeTearDown]
    public void Dispose()
    {
        _powerShellAccessor.Dispose();
    }
}