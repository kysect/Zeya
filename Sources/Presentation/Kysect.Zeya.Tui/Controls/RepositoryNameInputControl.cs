using Kysect.Zeya.Dtos;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public class RepositoryNameInputControl(IAnsiConsole console)
{
    public GithubRepositoryNameDto AskDto()
    {
        string repositoryFullName = console.Ask<string>("Repository (format: org/repo):");
        if (!repositoryFullName.Contains('/'))
            throw new ArgumentException("Incorrect repository format");

        string[] parts = repositoryFullName.Split('/', 2);
        return new GithubRepositoryNameDto(parts[0], parts[1]);
    }
}