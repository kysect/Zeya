using Kysect.Zeya.Dtos;
using Refit;

namespace Kysect.Zeya.Client.Abstractions;

public interface IPolicyService
{
    [Post("/Policies")]
    Task<ValidationPolicyDto> CreatePolicy(string name, string content);

    [Get("/Policies/{id}")]
    Task<ValidationPolicyDto> GetPolicy(Guid id);

    [Get("/Policies")]
    Task<IReadOnlyCollection<ValidationPolicyDto>> GetPolicies();

    [Get("/Policies/{policyId}/DiagnosticsTables")]
    Task<IReadOnlyCollection<RepositoryDiagnosticTableRow>> GetDiagnosticsTable(Guid policyId);
}