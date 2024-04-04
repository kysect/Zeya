using FluentAssertions;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.Tests.Tools;
using Kysect.Zeya.Tui.Controls;
using Spectre.Console.Testing;

namespace Kysect.Zeya.Tests.Presentation.Tui.Controls;

public class RepositoryDiagnosticTableTests
{
    [Fact]
    public void Show_TwoRowWithRules_ReturnExpectedResult()
    {
        using var console = new TestConsole();

        var repositoryDiagnosticTable = new RepositoryDiagnosticTable();
        List<RepositoryDiagnosticTableRow> rows =
        [
            new RepositoryDiagnosticTableRow(Guid.NewGuid(), "Owner/Repository", new Dictionary<string, string>() { ["SRC0001"] = "Warning", ["SRC0002"] = "Warning" }),
            new RepositoryDiagnosticTableRow(Guid.NewGuid(), "Owner/Repository2", new Dictionary<string, string>() { ["SRC0001"] = "Warning" })
        ];

        console.Write(repositoryDiagnosticTable.CreateGrid(rows));

        string consoleOutput = console.Output;

        // TODO: need to report bug to Spectre.Console
        consoleOutput = consoleOutput.NormalizeEndLines();
        consoleOutput.Should().Be("""
                                  Repository         SRC0001  SRC0002
                                  Owner/Repository   Warning  Warning
                                  Owner/Repository2  Warning  N/A    
                                  
                                  """);
    }
}