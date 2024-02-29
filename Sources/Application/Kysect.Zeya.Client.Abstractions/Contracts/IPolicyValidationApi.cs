using Refit;

namespace Kysect.Zeya.Client.Abstractions.Contracts;

public interface IPolicyValidationApi
{
    [Post("/Policy/{PolicyId}/Validate")]
    Task Validate(Guid policyId);

    [Post("/Policy/{PolicyId}/Create-fix")]
    Task CreateFix(Guid policyId, string repositoryOwner, string repositoryName);
}