using Refit;

namespace Kysect.Zeya.Client.Abstractions;

public interface IPolicyValidationService
{
    [Post("/Policies/{PolicyId}/Validate")]
    Task Validate(Guid policyId);

    [Post("/Policies/{PolicyId}/{RepositoryId}/Fix")]
    Task CreateFix(Guid policyId, Guid repositoryId);

    [Post("/Policies/{PolicyId}/{RepositoryId}/PreviewChanges")]
    Task<string> PreviewChanges(Guid policyId, Guid repositoryId);
}