using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.IntegrationManager.Models;
using Spectre.Console;

namespace Kysect.Zeya.Tui.Controls;

public class RepositoryDiagnosticTable
{
    public Grid CreateGrid(IReadOnlyCollection<RepositoryDiagnosticTableRow> rows)
    {
        rows.ThrowIfNull();

        var grid = new Grid();
        string[] ruleIds = rows.SelectMany(r => r.Diagnostics.Keys).Distinct().ToArray();

        grid.AddColumns(ruleIds.Length + 1);
        grid.AddRow(ruleIds.Prepend("Repository").ToArray());

        foreach (RepositoryDiagnosticTableRow tableRow in rows)
        {
            grid.AddRow(ruleIds
                .Select(ruleId => tableRow.Diagnostics.GetValueOrDefault(ruleId, "N/A"))
                .Prepend(tableRow.RepositoryName)
                .ToArray());
        }

        return grid;
    }
}