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

    public async Task<ValidationPolicyRepositoryDto> AddRemoteRepository(Guid policyId, string remoteHttpUrl, string? solutionPathMask)
    {
        ValidationPolicyEntity policy = await context.ValidationPolicies.GetAsync(policyId);

        if (solutionPathMask is null)
            solutionPathMask = LocalRepositorySolutionManager.DefaultMask;

        var repository = new ValidationPolicyRepository(Guid.NewGuid(), policyId, ValidationPolicyRepositoryType.RemoteHttps, remoteHttpUrl, solutionPathMask);
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

    public async Task<IReadOnlyCollection<ValidationPolicyRepositoryActionDto>> GetActions(Guid policyId, Guid repositoryId)
    {
        ValidationPolicyEntity policy = await context.ValidationPolicies.GetAsync(policyId);

        var actionMessages = await context
            .ValidationPolicyRepositoryActions
            .Join(context.ValidationPolicyRepositoryActionMessages,
                r => r.ActionId,
                d => d.ActionId,
                (a, m) => new { Action = a, Message = m })
            .Where(t => t.Action.ValidationPolicyRepositoryId == repositoryId)
            .ToListAsync();

        List<ValidationPolicyRepositoryActionDto> result = new();
        foreach (var actionMessageGroup in actionMessages.GroupBy(a => a.Message.ActionId))
        {
            var sampleValue = actionMessageGroup.First();
            List<string> messages = actionMessageGroup.Select(m => m.Message.Message).ToList();
            var dto = new ValidationPolicyRepositoryActionDto(sampleValue.Action.ActionId, sampleValue.Action.Title, sampleValue.Action.PerformedAt, messages);
            result.Add(dto);
        }

        return result;
    }
}