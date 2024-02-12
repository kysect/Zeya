using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.Tui.Controls;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeRepositoriesCommand(
    RepositorySelectorControl repositorySelector,
    RepositoryValidationService repositoryValidationService) : ITuiCommand
{
    public string Name => "Analyze repositories";

    public void Execute()
    {
        IReadOnlyCollection<ILocalRepository> repositories = repositorySelector.SelectRepositories();
        // TODO: remove hardcoded value
        repositoryValidationService.Analyze(repositories, "Demo-validation.yaml");
    }
}