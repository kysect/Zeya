using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public static class RepositoryInputControl
{
    public static GithubRepository Ask()
    {
        string repositoryFullName = AnsiConsole.Ask<string>("Repository (format: org/repo):");
        if (!repositoryFullName.Contains('/'))
            throw new ArgumentException("Incorrect repository format");

        string[] parts = repositoryFullName.Split('/', 2);
        return new GithubRepository(parts[0], parts[1]);
    }
}