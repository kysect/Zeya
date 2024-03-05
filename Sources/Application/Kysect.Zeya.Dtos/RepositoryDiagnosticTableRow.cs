namespace Kysect.Zeya.Dtos;

public record RepositoryDiagnosticTableRow(Guid Id, string RepositoryName, Dictionary<string, string> Diagnostics);