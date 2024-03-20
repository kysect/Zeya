using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.DataAccess.EntityFramework.Tools;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.EntityFrameworkCore;

namespace Kysect.Zeya.Application.LocalHandling;

public class PolicyRepositoryService(
    ValidationPolicyRepositoryFactory repositoryFactory,
    ZeyaDbContext context)
    : IPolicyRepositoryService
{
    public async Task<IReadOnlyCollection<ValidationPolicyRepositoryDto>> GetRepositories(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepository> repositories = await context
            .ValidationPolicyRepositories
            .Where(r => r.ValidationPolicyId == policyId)
            .ToListAsync();

        return repositories
            .Select(r => repositoryFactory.Create(r).ToDto())
            .ToList();
    }

    public async Task<ValidationPolicyRepositoryDto> AddGithubRepository(Guid policyId, string githubOwner, string githubRepository, string? solutionPathMask)
    {
        ValidationPolicyEntity policy = await context.ValidationPolicies.GetAsync(policyId);

        if (solutionPathMask is null)
            solutionPathMask = LocalRepositorySolutionManager.DefaultMask;

        var githubRepositoryName = new GithubRepositoryName(githubOwner, githubRepository);
        var repository = new ValidationPolicyRepository(Guid.NewGuid(), policyId, ValidationPolicyRepositoryType.Github, githubRepositoryName.FullName, solutionPathMask);
        context.ValidationPolicyRepositories.Add(repository);
        await context.SaveChangesAsync();

        return repositoryFactory.Create(repository).ToDto();
    }

    public async Task<ValidationPolicyRepositoryDto> AddLocalRepository(Guid policyId, string path, string? solutionPathMask)
    {
        ValidationPolicyEntity policy = await context.ValidationPolicies.GetAsync(policyId);

        if (solutionPathMask is null)
            solutionPathMask = LocalRepositorySolutionManager.DefaultMask;

        var repository = new ValidationPolicyRepository(Guid.NewGuid(), policyId, ValidationPolicyRepositoryType.Local, path, solutionPathMask);
        context.ValidationPolicyRepositories.Add(repository);
        await context.SaveChangesAsync();

        return repositoryFactory.Create(repository).ToDto();
    }
}