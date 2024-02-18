namespace Kysect.Zeya.IntegrationManager.Models;

public record RepositoryDiagnosticTableRow(string RepositoryName, Dictionary<string, string> Diagnostics);