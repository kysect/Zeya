using Kysect.CommonLib.Exceptions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Kysect.Zeya.Application;

public class GithubRepositoryProvider : IGithubRepositoryProvider
{
    private readonly IFileSystem _fileSystem;
    private readonly IGithubIntegrationService _githubIntegrationService;
    private readonly ILocalStoragePathFactory _localStoragePathFactory;
    private readonly DotnetSolutionModifierFactory _solutionModifierFactory;
    private readonly IRepositoryFetcher _repositoryFetcher;
    private readonly ILogger<GithubRepositoryProvider> _logger;

    public GithubRepositoryProvider(
        IFileSystem fileSystem,
        ILogger<GithubRepositoryProvider> logger,
        IGithubIntegrationService githubIntegrationService,
        ILocalStoragePathFactory localStoragePathFactory,
        DotnetSolutionModifierFactory solutionModifierFactory,
        IRepositoryFetcher repositoryFetcher)
    {
        _fileSystem = fileSystem;
        _logger = logger;
        _githubIntegrationService = githubIntegrationService;
        _localStoragePathFactory = localStoragePathFactory;
        _solutionModifierFactory = solutionModifierFactory;
        _repositoryFetcher = repositoryFetcher;
    }

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
        return new LocalRepository(path, solutionSearchMask, _fileSystem, _solutionModifierFactory);
    }

    private ILocalRepository CreateRemoteRepositoryCache(string remoteHttpsUrl, string solutionSearchMask)
    {
        _logger.LogInformation("Loading repository {RemoteHttpsUrl}", remoteHttpsUrl);

        string repositoryName = remoteHttpsUrl.Split('/').Last();
        // TODO: This is kind of hack because we don't have access to cache directory path
        string repositoryPath = _localStoragePathFactory.GetPathToRepository(new GithubRepository("RemoteRepository", repositoryName));

        var gitRepository = new CustomRemoteGitRepository(repositoryName, remoteHttpsUrl);
        _repositoryFetcher.EnsureRepositoryUpdated(repositoryPath, gitRepository);

        return new LocalRepository(
            repositoryPath,
            solutionSearchMask,
            _fileSystem,
            _solutionModifierFactory);
    }

    private LocalGithubRepository CreateGithubRepositoryAccessor(GithubRepositoryName githubRepositoryName, string solutionSearchMask)
    {
        _logger.LogInformation("Loading repository {Repository}", githubRepositoryName.FullName);
        _githubIntegrationService.CloneOrUpdate(githubRepositoryName);
        string repositoryRootPath = _localStoragePathFactory.GetPathToRepository(new GithubRepository(githubRepositoryName.Owner, githubRepositoryName.Name));

        return new LocalGithubRepository(
            githubRepositoryName,
            repositoryRootPath,
            solutionSearchMask,
            _githubIntegrationService,
            _fileSystem,
            _solutionModifierFactory);
    }
}