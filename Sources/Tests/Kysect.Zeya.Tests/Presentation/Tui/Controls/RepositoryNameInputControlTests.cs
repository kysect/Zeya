using FluentAssertions;
using Kysect.Zeya.Client.Abstractions.Models;
using Kysect.Zeya.Tui.Controls;
using Spectre.Console.Testing;

namespace Kysect.Zeya.Tests.Presentation.Tui.Controls;

public class RepositoryNameInputControlTests
{
    [Fact]
    public void Ask_WhenCalled_ReturnsRepositoryName()
    {
        string repositoryFullName = "org/repo";
        string expectedOwner = "org";
        string expectedName = "repo";
        var console = new TestConsole();
        var repositoryNameInputControl = new RepositoryNameInputControl(console);

        console.Input.PushText(repositoryFullName);
        console.Input.PushKey(ConsoleKey.Enter);
        GithubRepositoryNameDto result = repositoryNameInputControl.AskDto();

        result.Should().Be(new GithubRepositoryNameDto(expectedOwner, expectedName));
    }
}