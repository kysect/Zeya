using Kysect.TerminalUserInterface.Commands;
using Kysect.TerminalUserInterface.Menu;
using Kysect.Zeya.Tui.Commands;

namespace Kysect.Zeya.Tui;

public record RootMenu(
    AnalyzeRepositoriesCommand AnalyzeRepositoriesCommand,
    AnalyzeAndFixRepositoryCommand AnalyzeAndFixRepositoryCommand,
    AnalyzeAndFixAndCreatePullRequestRepositoryCommand AnalyzeAndFixAndCreatePullRequestRepositoryCommand,
    AddPolicyCommand AddPolicyCommand,
    AddRepositoryCommand AddRepositoryCommand,
    AnalyzeAndSaveToDatabaseCommand AnalyzeAndSaveToDatabaseCommand) : ITuiMenu
{
    public string Name => "Root menu";

    public IReadOnlyCollection<ITuiCommand> GetMenuItems()
    {
        return new List<ITuiCommand>
        {
            AnalyzeRepositoriesCommand,
            AnalyzeAndFixRepositoryCommand,
            AnalyzeAndFixAndCreatePullRequestRepositoryCommand,
            AddPolicyCommand,
            AddRepositoryCommand,
            AnalyzeAndSaveToDatabaseCommand
        };
    }
}