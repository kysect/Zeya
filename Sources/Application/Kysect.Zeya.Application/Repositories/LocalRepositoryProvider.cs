using Kysect.CommonLib.Exceptions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.Zeya.AdoIntegration.Abstraction;
using Kysect.Zeya.Application.Repositories.Ado;
using Kysect.Zeya.Application.Repositories.Github;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Ado;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.Application.Repositories;

public class LocalRepositoryProvider(
    IFileSystem fileSystem,
    ILogger<LocalRepositoryProvider> logger,
    IRemoteHostIntegrationServiceFactory remoteHostIntegrationServiceFactory,
    ILocalStoragePathFactory localStoragePathFactory,
    DotnetSolutionModifierFactory solutionModifierFactory)
{
    public ILocalRepository InitializeRepository(IValidationPolicyRepository repository)
    {
        return repository switch
        {
            GithubValidationPolicyRepository githubRepository => CreateGithubRepositoryAccessor(githubRepository),
            LocalValidationPolicyRepository localRepository => GetLocalRepository(localRepository.Path, repository.SolutionPathMask),
            AdoValidationPolicyRepository adoRepository => CreateLocalAdoRepository(adoRepository),
            RemoteHttpsValidationPolicyRepository remoteHttpsValidationPolicyRepository => CreateRemoteRepositoryCache(remoteHttpsValidationPolicyRepository),
            _ => throw SwitchDefaultExceptions.OnUnexpectedType(repository)
        };
    }

    public ILocalRepository GetLocalRepository(string path, string solutionSearchMask)
    {
        return new LocalRepository(path, solutionSearchMask, fileSystem, solutionModifierFactory);
    }

    private ILocalRepository CreateRemoteRepositoryCache(RemoteHttpsValidationPolicyRepository remoteHttpsValidationPolicyRepository)
    {
        logger.LogInformation("Loading repository {RemoteHttpsUrl}", remoteHttpsValidationPolicyRepository.RemoteHttpsUrl);

        string remoteHttpsUrl = remoteHttpsValidationPolicyRepository.RemoteHttpsUrl;
        IGitIntegrationService gitIntegrationService = remoteHostIntegrationServiceFactory.GetGitService(remoteHttpsValidationPolicyRepository);

        string repositoryName = remoteHttpsUrl.Split('/').Last();
        // TODO: This is kind of hack because we don't have access to cache directory path
        string repositoryPath = localStoragePathFactory.GetPathToRepository(new GithubRepository("RemoteRepository", repositoryName));

        var gitRepository = new CustomRemoteGitRepository(repositoryName, remoteHttpsUrl);
        gitIntegrationService.EnsureRepositoryUpdated(repositoryPath, gitRepository);

        return new LocalRepository(
            repositoryPath,
            remoteHttpsValidationPolicyRepository.SolutionPathMask,
            fileSystem,
            solutionModifierFactory);
    }

    private LocalGithubRepository CreateGithubRepositoryAccessor(GithubValidationPolicyRepository githubRepository)
    {
        var githubRepositoryName = new GithubRepositoryName(githubRepository.Owner, githubRepository.Name);
        var repository = new GithubRepository(githubRepository.Owner, githubRepository.Name);
        logger.LogInformation("Loading repository {Repository}", githubRepositoryName.FullName);

        string pathToRepository = localStoragePathFactory.GetPathToRepository(repository);
        IGitIntegrationService gitIntegrationService = remoteHostIntegrationServiceFactory.GetGitService(githubRepository);
        gitIntegrationService.EnsureRepositoryUpdated(pathToRepository, repository);
        IGithubIntegrationService integrationService = remoteHostIntegrationServiceFactory.GetGithubService(githubRepository);

        return new LocalGithubRepository(
            githubRepositoryName,
            pathToRepository,
            githubRepository.SolutionPathMask,
            integrationService,
            fileSystem,
            solutionModifierFactory);
    }

    private LocalAdoRepository CreateLocalAdoRepository(AdoValidationPolicyRepository adoRepository)
    {
        logger.LogInformation("Loading repository {RemoteHttpsUrl}", adoRepository.RemoteHttpsUrl);

        string remoteHttpsUrl = adoRepository.RemoteHttpsUrl;
        IGitIntegrationService gitIntegrationService = remoteHostIntegrationServiceFactory.GetGitService(adoRepository);
        IAdoIntegrationService adoIntegrationService = remoteHostIntegrationServiceFactory.GetAdoService(adoRepository);

        string repositoryName = remoteHttpsUrl.Split('/').Last();
        // TODO: This is kind of hack because we don't have access to cache directory path
        string repositoryPath = localStoragePathFactory.GetPathToRepository(new GithubRepository("RemoteRepository", repositoryName));

        var gitRepository = new CustomRemoteGitRepository(repositoryName, remoteHttpsUrl);
        gitIntegrationService.EnsureRepositoryUpdated(repositoryPath, gitRepository);

        return new LocalAdoRepository(
            repositoryPath,
            adoRepository.RemoteHttpsUrl,
            adoRepository.SolutionPathMask,
            adoIntegrationService,
            fileSystem,
            solutionModifierFactory);
    }
}