namespace Kysect.Zeya.Client.Abstractions.Models;

public record RepositoryDiagnosticTableRow(string RepositoryName, Dictionary<string, string> Diagnostics);