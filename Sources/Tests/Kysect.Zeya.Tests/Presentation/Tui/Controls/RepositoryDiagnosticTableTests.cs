using FluentAssertions;
using Kysect.Zeya.Dtos;
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
            new RepositoryDiagnosticTableRow("Owner", "Repository", new Dictionary<string, string>() { ["SRC0001"] = "Warning", ["SRC0002"] = "Warning" }),
            new RepositoryDiagnosticTableRow("Owner", "Repository2", new Dictionary<string, string>() { ["SRC0001"] = "Warning" })
        ];

        console.Write(repositoryDiagnosticTable.CreateGrid(rows));

        console.Output.Should().Be("""
                                   Repository         SRC0001  SRC0002
                                   Owner/Repository   Warning  Warning
                                   Owner/Repository2  Warning  N/A    
                                   
                                   """);
    }
}