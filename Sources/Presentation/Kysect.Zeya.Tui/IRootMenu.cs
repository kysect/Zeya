using Kysect.TerminalUserInterface.Menu;
using Kysect.Zeya.Tui.Commands;
using Kysect.Zeya.Tui.Commands.Policies;

namespace Kysect.Zeya.Tui;

public interface IRootMenu : ITuiMainMenu
{
    [TuiName("Policy management menu")]
    IPolicyMenu PolicyMenu { get; }


    [TuiName("Analyze Repositories")]
    AnalyzeRepositoriesCommand AnalyzeRepositoriesCommand { get; }
    [TuiName("Analyze and Fix Repository")]
    AnalyzeAndFixRepositoryCommand AnalyzeAndFixRepositoryCommand { get; }
    [TuiName("Analyze And Fix And Create Pull Request")]
    AnalyzeAndFixAndCreatePullRequestRepositoryCommand AnalyzeAndFixAndCreatePullRequestRepositoryCommand { get; }
    [TuiName("Analyze And Save To Database")]
    AnalyzeAndSaveToDatabaseCommand AnalyzeAndSaveToDatabaseCommand { get; }
    [TuiName("Show Repository Validation Report")]
    ShowRepositoryValidationReportCommand ShowRepositoryValidationReportCommand { get; }
}
