using Kysect.Zeya.Client.Abstractions.Models;

namespace Kysect.Zeya.Client.Abstractions.Contracts;

public interface IValidationPolicyApi
{
    ValidationPolicyDto CreatePolicy(string name, string content);
    IReadOnlyCollection<ValidationPolicyDto> ReadPolicies();
    ValidationPolicyRepositoryDto AddRepository(Guid policyId, string githubOwner, string githubRepository);
    IReadOnlyCollection<RepositoryDiagnosticTableRow> GetDiagnosticsTable(Guid policyId);
}