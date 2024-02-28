using Kysect.Zeya.Client.Abstractions.Models;
using Refit;

namespace Kysect.Zeya.Client.Abstractions.Contracts;

public interface IValidationPolicyApi
{
    [Post("/ValidationPolicy")]
    Task<ValidationPolicyDto> CreatePolicy(string name, string content);

    [Get("/ValidationPolicy/{id}")]
    Task<ValidationPolicyDto> GetPolicy(Guid id);

    [Get("/ValidationPolicy")]
    Task<IReadOnlyCollection<ValidationPolicyDto>> GetPolicies();

    [Get("/ValidationPolicy/DiagnosticsTables")]
    Task<IReadOnlyCollection<RepositoryDiagnosticTableRow>> GetDiagnosticsTable(Guid policyId);
}