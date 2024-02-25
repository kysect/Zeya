using FluentAssertions;
using Kysect.Zeya.Client.Local;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tui.Commands.Policies;
using Spectre.Console.Testing;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Presentation.Tui.Commands;

public class AddPolicyCommandTests : IDisposable
{
    private readonly ValidationPolicyService _validationPolicyService;
    private readonly MockFileSystem _mockFileSystem;
    private readonly TestConsole _console;

    public AddPolicyCommandTests()
    {
        _validationPolicyService = new ValidationPolicyService(ZeyaDbContextProvider.CreateContext());
        _mockFileSystem = new MockFileSystem();
        _console = new TestConsole();
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
        var addPolicyCommand = new AddPolicyCommand(new ValidationPolicyLocalClient(_validationPolicyService), _mockFileSystem, _console);

        addPolicyCommand.Execute();
        IReadOnlyCollection<ValidationPolicyEntity> policies = await _validationPolicyService.ReadPolicies();

        policies.Should().HaveCount(1);
        policies.First().Name.Should().Be("Nuget");
        policies.First().Content.Should().Be(policyContent);
    }

    public void Dispose()
    {
        _console.Dispose();
    }
}
