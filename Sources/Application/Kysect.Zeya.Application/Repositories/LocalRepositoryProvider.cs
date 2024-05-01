using Kysect.CommonLib.Exceptions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.Application.Repositories;

public class LocalRepositoryProvider(
    IFileSystem fileSystem,
    ILogger<LocalRepositoryProvider> logger,
    IGithubIntegrationService githubIntegrationService,
    ILocalStoragePathFactory localStoragePathFactory,
    DotnetSolutionModifierFactory solutionModifierFactory,
    IRepositoryFetcher repositoryFetcher)
{
    public ILocalRepository InitializeRepository(IValidationPolicyRepository repository)
    {
        return repository switch
        {
            GithubValidationPolicyRepository githubRepository => CreateGithubRepositoryAccessor(new GithubRepositoryName(githubRepository.Owner, githubRepository.Name), repository.SolutionPathMask),
            LocalValidationPolicyRepository localRepository => GetLocalRepository(localRepository.Path, repository.SolutionPathMask),
            RemoteHttpsValidationPolicyRepository remoteHttpsValidationPolicyRepository => CreateRemoteRepositoryCache(remoteHttpsValidationPolicyRepository.RemoteHttpsUrl, repository.SolutionPathMask),
            _ => throw SwitchDefaultExceptions.OnUnexpectedType(repository)
        };
    }

    public LocalGithubRepository GetGithubRepository(string owner, string repository)
    {
        return CreateGithubRepositoryAccessor(new GithubRepositoryName(owner, repository), LocalRepositorySolutionManager.DefaultMask);
    }

    public ILocalRepository GetLocalRepository(string path, string solutionSearchMask)
    {
        return new LocalRepository(path, solutionSearchMask, fileSystem, solutionModifierFactory);
    }

    private ILocalRepository CreateRemoteRepositoryCache(string remoteHttpsUrl, string solutionSearchMask)
    {
        logger.LogInformation("Loading repository {RemoteHttpsUrl}", remoteHttpsUrl);

        string repositoryName = remoteHttpsUrl.Split('/').Last();
        // TODO: This is kind of hack because we don't have access to cache directory path
        string repositoryPath = localStoragePathFactory.GetPathToRepository(new GithubRepository("RemoteRepository", repositoryName));

        var gitRepository = new CustomRemoteGitRepository(repositoryName, remoteHttpsUrl);
        repositoryFetcher.EnsureRepositoryUpdated(repositoryPath, gitRepository);

        return new LocalRepository(
            repositoryPath,
            solutionSearchMask,
            fileSystem,
            solutionModifierFactory);
    }

    private LocalGithubRepository CreateGithubRepositoryAccessor(GithubRepositoryName githubRepositoryName, string solutionSearchMask)
    {
        logger.LogInformation("Loading repository {Repository}", githubRepositoryName.FullName);

        var repository = new GithubRepository(githubRepositoryName.Owner, githubRepositoryName.Name);
        string pathToRepository = localStoragePathFactory.GetPathToRepository(repository);
        repositoryFetcher.EnsureRepositoryUpdated(pathToRepository, repository);

        return new LocalGithubRepository(
            githubRepositoryName,
            pathToRepository,
            solutionSearchMask,
            githubIntegrationService,
            fileSystem,
            solutionModifierFactory);
    }
}