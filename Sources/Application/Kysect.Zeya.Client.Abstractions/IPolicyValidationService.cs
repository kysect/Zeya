using Refit;

namespace Kysect.Zeya.Client.Abstractions;

public interface IPolicyValidationService
{
    [Post("/Policies/{PolicyId}/Validate")]
    Task Validate(Guid policyId);

    [Post("/Policies/{PolicyId}/Create-fix")]
    Task CreateFix(Guid policyId, string repositoryOwner, string repositoryName);
}