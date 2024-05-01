using Kysect.CommonLib.Exceptions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.Zeya.Application.Repositories.Github;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.Application.Repositories;

public class LocalRepositoryProvider(
    IFileSystem fileSystem,
    ILogger<LocalRepositoryProvider> logger,
    IGitIntegrationService gitIntegrationService,
    IGithubIntegrationServiceFactory githubIntegrationServiceFactory,
    ILocalStoragePathFactory localStoragePathFactory,
    DotnetSolutionModifierFactory solutionModifierFactory)
{
    public ILocalRepository InitializeRepository(IValidationPolicyRepository repository)
    {
        return repository switch
        {
            GithubValidationPolicyRepository githubRepository => CreateGithubRepositoryAccessor(githubRepository),
            LocalValidationPolicyRepository localRepository => GetLocalRepository(localRepository.Path, repository.SolutionPathMask),
            RemoteHttpsValidationPolicyRepository remoteHttpsValidationPolicyRepository => CreateRemoteRepositoryCache(remoteHttpsValidationPolicyRepository.RemoteHttpsUrl, repository.SolutionPathMask),
            _ => throw SwitchDefaultExceptions.OnUnexpectedType(repository)
        };
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
        gitIntegrationService.EnsureRepositoryUpdated(repositoryPath, gitRepository);

        return new LocalRepository(
            repositoryPath,
            solutionSearchMask,
            fileSystem,
            solutionModifierFactory);
    }

    private LocalGithubRepository CreateGithubRepositoryAccessor(GithubValidationPolicyRepository githubRepository)
    {
        var githubRepositoryName = new GithubRepositoryName(githubRepository.Owner, githubRepository.Name);
        var repository = new GithubRepository(githubRepository.Owner, githubRepository.Name);
        logger.LogInformation("Loading repository {Repository}", githubRepositoryName.FullName);

        string pathToRepository = localStoragePathFactory.GetPathToRepository(repository);
        gitIntegrationService.EnsureRepositoryUpdated(pathToRepository, repository);
        IGithubIntegrationService integrationService = githubIntegrationServiceFactory.GetService(githubRepository);

        return new LocalGithubRepository(
            githubRepositoryName,
            pathToRepository,
            githubRepository.SolutionPathMask,
            integrationService,
            fileSystem,
            solutionModifierFactory);
    }
}