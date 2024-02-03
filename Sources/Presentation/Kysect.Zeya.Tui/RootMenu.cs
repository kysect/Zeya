using Kysect.TerminalUserInterface.Commands;
using Kysect.TerminalUserInterface.Menu;
using Kysect.Zeya.Tui.Commands;

namespace Kysect.Zeya.Tui;

public record RootMenu(
    AnalyzeRepositoryCommand AnalyzeRepositoryCommand,
    AnalyzeAndFixRepositoryCommand AnalyzeAndFixRepositoryCommand,
    AnalyzeAndFixAndCreatePullRequestRepositoryCommand AnalyzeAndFixAndCreatePullRequestRepositoryCommand,
    AnalyzerGithubOrganization AnalyzerGithubOrganizationCommand) : ITuiMenu
{
    public string Name => "Root menu";

    public IReadOnlyCollection<ITuiCommand> GetMenuItems()
    {
        return [
            AnalyzeRepositoryCommand,
            AnalyzeAndFixRepositoryCommand,
            AnalyzeAndFixAndCreatePullRequestRepositoryCommand,
            AnalyzerGithubOrganizationCommand];
    }
}