using FluentAssertions;
using Kysect.Zeya.Application.DatabaseQueries;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tui.Commands.Policies;
using Spectre.Console.Testing;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Presentation.Tui.Commands;

public class AddPolicyCommandTests : IDisposable
{
    private readonly MockFileSystem _mockFileSystem;
    private readonly TestConsole _console;
    private readonly PolicyService _policyService;

    public AddPolicyCommandTests()
    {
        ZeyaDbContext dbContext = ZeyaDbContextTestProvider.CreateContext();
        var validationPolicyService = new ValidationPolicyDatabaseQueries(dbContext);
        _mockFileSystem = new MockFileSystem();
        _console = new TestConsole();
        _policyService = new PolicyService(validationPolicyService, dbContext);
    }

    [Fact]
    public async Task AddPolicyCommandTests_WhenCalled_ShouldAddPolicy()
    {
        string filePath = "File.yaml";
        string policyContent = "Policy content";
        _console.Input.PushText("Nuget");
        _console.Input.PushKey(ConsoleKey.Enter);
        _console.Input.PushText(filePath);
        _console.Input.PushKey(ConsoleKey.Enter);
        _mockFileSystem.AddFile(filePath, new MockFileData(policyContent));
        var addPolicyCommand = new AddPolicyCommand(_policyService, _mockFileSystem, _console);

        addPolicyCommand.Execute();
        IReadOnlyCollection<ValidationPolicyDto> policies = await _policyService.GetPolicies();

        policies.Should().HaveCount(1);
        policies.First().Name.Should().Be("Nuget");
        policies.First().Content.Should().Be(policyContent);
    }

    public void Dispose()
    {
        _console.Dispose();
    }
}
