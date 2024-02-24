using Kysect.Zeya.Client.Abstractions.Models;
using Refit;

namespace Kysect.Zeya.Client.Abstractions.Contracts;

public interface IValidationPolicyApi
{
    [Put("ValidationPolicy/Policies")]
    ValidationPolicyDto CreatePolicy(string name, string content);
    [Get("ValidationPolicy/Policies")]
    IReadOnlyCollection<ValidationPolicyDto> ReadPolicies();
    [Post("ValidationPolicy/Repositories")]
    ValidationPolicyRepositoryDto AddRepository(Guid policyId, string githubOwner, string githubRepository);
    [Get("ValidationPolicy/DiagnosticsTables")]
    IReadOnlyCollection<RepositoryDiagnosticTableRow> GetDiagnosticsTable(Guid policyId);
    [Get("ValidationPolicy/Repositories")]
    IReadOnlyCollection<ValidationPolicyRepositoryDto> GetRepositories(Guid policyId);
}