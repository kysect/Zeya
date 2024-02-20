using Kysect.TerminalUserInterface.Menu;
using Kysect.Zeya.Tui.Commands;

namespace Kysect.Zeya.Tui;

public interface IRootMenu : ITuiMainMenu
{
    [TuiName("Analyze Repositories")]
    AnalyzeRepositoriesCommand AnalyzeRepositoriesCommand { get; }
    [TuiName("Analyze and Fix Repository")]
    AnalyzeAndFixRepositoryCommand AnalyzeAndFixRepositoryCommand { get; }
    [TuiName("Analyze And Fix And Create Pull Request")]
    AnalyzeAndFixAndCreatePullRequestRepositoryCommand AnalyzeAndFixAndCreatePullRequestRepositoryCommand { get; }
    [TuiName("Add Policy")]
    AddPolicyCommand AddPolicyCommand { get; }
    [TuiName("Add Repository")]
    AddRepositoryCommand AddRepositoryCommand { get; }
    [TuiName("Analyze And Save To Database")]
    AnalyzeAndSaveToDatabaseCommand AnalyzeAndSaveToDatabaseCommand { get; }
    [TuiName("Show Repository Validation Report")]
    ShowRepositoryValidationReportCommand ShowRepositoryValidationReportCommand { get; }
}
