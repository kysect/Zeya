using Kysect.CommonLib.Exceptions;
using Kysect.TerminalUserInterface.Controls.Selection;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.IntegrationManager;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public class RepositorySelector
{
    private readonly IGithubRepositoryProvider _githubRepositoryProvider;

    public RepositorySelector(IGithubRepositoryProvider githubRepositoryProvider)
    {
        _githubRepositoryProvider = githubRepositoryProvider;
    }

    public IReadOnlyCollection<IClonedRepository> SelectRepositories()
    {
        string? selectionType = new SelectionPrompt<string>()
            .AddChoices("Local git repository", "Github repository", "Github organization")
            .PromptWithCancellation();

        if (selectionType is null)
            return [];

        if (selectionType == "Local git repository")
        {
            string repositoryFullName = AnsiConsole.Ask<string>("Local repository path:");
            IClonedRepository clonedRepository = _githubRepositoryProvider.GetLocalRepository(repositoryFullName);
            return [clonedRepository];
        }

        if (selectionType == "Github repository")
        {
            GithubRepositoryName githubRepositoryName = RepositoryNameInputControl.Ask();
            ClonedGithubRepository clonedGithubRepository = _githubRepositoryProvider.GetGithubRepository(githubRepositoryName.Owner, githubRepositoryName.Name);
            return [clonedGithubRepository];
        }

        if (selectionType == "Github organization")
        {
            string organization = AnsiConsole.Ask<string>("Organization for clone: ");
            // TODO: allow to input exclusion
            return _githubRepositoryProvider.GetGithubOrganizationRepositories(organization, []);
        }

        throw SwitchDefaultExceptions.OnUnexpectedValue(selectionType);
    }
}