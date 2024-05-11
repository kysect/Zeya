using Kysect.Zeya.Dtos;
using Refit;

namespace Kysect.Zeya.Client.Abstractions;

public interface IRepositoryDependenciesService
{
    [Get("/RepositoryDependencies/Tree")]
    Task<string> GetRepositoryDependenciesTree(Guid policyId);

    [Get("/RepositoryDependencies/ActionPlan")]
    Task<IReadOnlyCollection<FixingActionPlanRow>> GetFixingActionPlan(Guid policyId);
}