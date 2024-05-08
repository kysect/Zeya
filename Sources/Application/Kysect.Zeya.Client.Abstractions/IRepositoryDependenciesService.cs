using Refit;

namespace Kysect.Zeya.Client.Abstractions;

public interface IRepositoryDependenciesService
{
    [Get("/RepositoryDependencies")]
    Task<string> GetRepositoryDependenciesTree(Guid policyId);
}