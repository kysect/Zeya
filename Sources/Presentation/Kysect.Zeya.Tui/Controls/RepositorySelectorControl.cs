using Kysect.CommonLib.Exceptions;
using Kysect.TerminalUserInterface.Controls.Selection;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.LocalRepositoryAccess;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public class RepositorySelectorControl
{
    private readonly IGithubRepositoryProvider _githubRepositoryProvider;

    public RepositorySelectorControl(IGithubRepositoryProvider githubRepositoryProvider)
    {
        _githubRepositoryProvider = githubRepositoryProvider;
    }

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
            ILocalRepository localRepository = _githubRepositoryProvider.GetLocalRepository(repositoryFullName);
            return [localRepository];
        }

        if (selectionType == "Github repository")
        {
            GithubRepositoryName githubRepositoryName = RepositoryNameInputControl.Ask();
            return [CreateGithubRepository(githubRepositoryName)];
        }

        if (selectionType == "Github organization")
        {
            string organization = AnsiConsole.Ask<string>("Organization for clone: ");
            // TODO: allow to input exclusion
            return _githubRepositoryProvider.GetGithubOrganizationRepositories(organization, []);
        }

        throw SwitchDefaultExceptions.OnUnexpectedValue(selectionType);
    }

    public LocalGithubRepository CreateGithubRepository(GithubRepositoryName githubRepositoryName)
    {
        return _githubRepositoryProvider.GetGithubRepository(githubRepositoryName.Owner, githubRepositoryName.Name);
    }
}