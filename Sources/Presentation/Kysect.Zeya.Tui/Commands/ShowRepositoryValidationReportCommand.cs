using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tui.Controls;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands;

public class ShowRepositoryValidationReportCommand(
    IPolicyService policyService,
    PolicySelectorControl policySelectorControl) : ITuiCommand
{
    public void Execute()
    {
        var repositoryDiagnosticTable = new RepositoryDiagnosticTable();
        ValidationPolicyDto? policy = policySelectorControl.SelectPolicy();
        if (policy is null)
            return;

        IReadOnlyCollection<RepositoryDiagnosticTableRow> diagnosticTableRows = policyService.GetDiagnosticsTable(policy.Id).Result;
        Grid grid = repositoryDiagnosticTable.CreateGrid(diagnosticTableRows);
        AnsiConsole.Write(grid);
    }
}