using Kysect.Zeya.Dtos;
using Refit;

namespace Kysect.Zeya.Client.Abstractions;

public interface IPolicyService
{
    [Post("/ValidationPolicy")]
    Task<ValidationPolicyDto> CreatePolicy(string name, string content);

    [Get("/ValidationPolicy/{id}")]
    Task<ValidationPolicyDto> GetPolicy(Guid id);

    [Get("/ValidationPolicy")]
    Task<IReadOnlyCollection<ValidationPolicyDto>> GetPolicies();

    [Get("/ValidationPolicy/{policyId}/DiagnosticsTables")]
    Task<IReadOnlyCollection<RepositoryDiagnosticTableRow>> GetDiagnosticsTable(Guid policyId);
}