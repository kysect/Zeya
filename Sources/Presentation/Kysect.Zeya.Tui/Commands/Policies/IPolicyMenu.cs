using Kysect.TerminalUserInterface.Menu;

namespace Kysect.Zeya.Tui.Commands.Policies;

public interface IPolicyMenu
{
    [TuiName("Add Policy")]
    AddPolicyCommand AddPolicyCommand { get; }
    [TuiName("Add Repository to policy")]
    AddRepositoryCommand AddRepositoryCommand { get; }
}