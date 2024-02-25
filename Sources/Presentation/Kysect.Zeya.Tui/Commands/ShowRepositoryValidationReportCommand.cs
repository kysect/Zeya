using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions.Contracts;
using Kysect.Zeya.Client.Abstractions.Models;
using Kysect.Zeya.Tui.Controls;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands;

public class ShowRepositoryValidationReportCommand(
    IValidationPolicyApi validationPolicyService,
    PolicySelectorControl policySelectorControl) : ITuiCommand
{
    public void Execute()
    {
        var repositoryDiagnosticTable = new RepositoryDiagnosticTable();
        ValidationPolicyDto? policy = policySelectorControl.SelectPolicy();
        if (policy is null)
            return;

        IReadOnlyCollection<RepositoryDiagnosticTableRow> diagnosticTableRows = validationPolicyService.GetDiagnosticsTable(policy.Id).Result;
        Grid grid = repositoryDiagnosticTable.CreateGrid(diagnosticTableRows);
        AnsiConsole.Write(grid);
    }
}