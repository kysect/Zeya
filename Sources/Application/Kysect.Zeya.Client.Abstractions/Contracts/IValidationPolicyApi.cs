using Kysect.Zeya.Client.Abstractions.Models;
using Refit;

namespace Kysect.Zeya.Client.Abstractions.Contracts;

public interface IValidationPolicyApi
{
    [Post("/ValidationPolicy/Policies")]
    Task<ValidationPolicyDto> CreatePolicy(string name, string content);

    [Get("/ValidationPolicy/Policies")]
    Task<IReadOnlyCollection<ValidationPolicyDto>> ReadPolicies();

    [Post("/ValidationPolicy/Repositories")]
    Task<ValidationPolicyRepositoryDto> AddRepository(Guid policyId, string githubOwner, string githubRepository);

    [Get("/ValidationPolicy/DiagnosticsTables")]
    Task<IReadOnlyCollection<RepositoryDiagnosticTableRow>> GetDiagnosticsTable(Guid policyId);

    [Get("/ValidationPolicy/Repositories")]
    Task<IReadOnlyCollection<ValidationPolicyRepositoryDto>> GetRepositories(Guid policyId);
}
