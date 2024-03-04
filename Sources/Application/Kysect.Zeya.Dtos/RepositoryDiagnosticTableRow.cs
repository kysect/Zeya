namespace Kysect.Zeya.Dtos;

public record RepositoryDiagnosticTableRow(string RepositoryOwner, string RepositoryName, Dictionary<string, string> Diagnostics);