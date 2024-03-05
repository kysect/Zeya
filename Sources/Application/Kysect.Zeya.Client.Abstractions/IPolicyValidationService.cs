using Refit;

namespace Kysect.Zeya.Client.Abstractions;

public interface IPolicyValidationService
{
    [Post("/Policies/{PolicyId}/Validate")]
    Task Validate(Guid policyId);

    [Post("/Policies/{PolicyId}/{RepositoryId}/Fix")]
    Task CreateFix(Guid policyId, Guid repositoryId);
}