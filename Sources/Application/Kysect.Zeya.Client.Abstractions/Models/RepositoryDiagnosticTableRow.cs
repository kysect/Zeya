namespace Kysect.Zeya.Client.Abstractions.Models;

public record RepositoryDiagnosticTableRow(string RepositoryOwner, string RepositoryName, Dictionary<string, string> Diagnostics);