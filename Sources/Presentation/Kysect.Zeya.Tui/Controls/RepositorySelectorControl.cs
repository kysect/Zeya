using Kysect.CommonLib.Exceptions;
using Kysect.TerminalUserInterface.Controls.Selection;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public class RepositorySelectorControl(
    IGithubRepositoryProvider githubRepositoryProvider,
    RepositoryNameInputControl repositoryNameInputControl)
{
    public IReadOnlyCollection<ILocalRepository> SelectRepositories()
    {
        string? selectionType = new SelectionPrompt<string>()
            .AddChoices("Local git repository", "Github repository", "Github organization")
            .PromptWithCancellation();

        if (selectionType is null)
            return [];

        if (selectionType == "Local git repository")
        {
            string repositoryFullName = AnsiConsole.Ask<string>("Local repository path:");
            ILocalRepository localRepository = githubRepositoryProvider.GetLocalRepository(repositoryFullName);
            return [localRepository];
        }

        if (selectionType == "Github repository")
        {
            GithubRepositoryName githubRepositoryName = repositoryNameInputControl.Ask();
            return [CreateGithubRepository(githubRepositoryName)];
        }

        if (selectionType == "Github organization")
        {
            string organization = AnsiConsole.Ask<string>("Organization for clone: ");
            // TODO: allow to input exclusion
            return githubRepositoryProvider.GetGithubOrganizationRepositories(organization, []);
        }

        throw SwitchDefaultExceptions.OnUnexpectedValue(selectionType);
    }

    public LocalGithubRepository CreateGithubRepository(GithubRepositoryName githubRepositoryName)
    {
        return githubRepositoryProvider.GetGithubRepository(githubRepositoryName.Owner, githubRepositoryName.Name);
    }
}