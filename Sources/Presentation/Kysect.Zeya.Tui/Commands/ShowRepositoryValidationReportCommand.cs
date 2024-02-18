using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.IntegrationManager.Models;
using Kysect.Zeya.Tui.Controls;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Commands;

public class ShowRepositoryValidationReportCommand(
    ValidationPolicyService validationPolicyService,
    PolicySelectorControl policySelectorControl) : ITuiCommand
{
    public string Name => "Show repository validation report";

    public void Execute()
    {
        var repositoryDiagnosticTable = new RepositoryDiagnosticTable();
        ValidationPolicyEntity? policy = policySelectorControl.SelectPolicy();
        if (policy is null)
            return;

        IReadOnlyCollection<RepositoryDiagnosticTableRow> diagnosticTableRows = validationPolicyService.GetDiagnosticsTable(policy.Id);
        Grid grid = repositoryDiagnosticTable.CreateGrid(diagnosticTableRows);
        AnsiConsole.Write(grid);
    }
}