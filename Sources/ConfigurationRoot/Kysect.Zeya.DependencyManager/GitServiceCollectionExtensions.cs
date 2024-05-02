using Kysect.CommonLib.DependencyInjection;
using Kysect.CommonLib.Exceptions;
using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.Zeya.AdoIntegration;
using Kysect.Zeya.AdoIntegration.Abstraction;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Application.Repositories.Ado;
using Kysect.Zeya.Application.Repositories.Git;
using Kysect.Zeya.Application.Repositories.Github;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.GitIntegration.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;

namespace Kysect.Zeya.DependencyManager;

public static class GitServiceCollectionExtensions
{
    public static IServiceCollection AddZeyaGitConfiguration(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddOptionsWithValidation<GitEnvironmentOptions>("GitEnvironmentOptions")
            .AddOptionsWithValidation<GitIntegrationCredentialOptions>("GitIntegrationCredentialOptions")
            .AddSingleton(sp =>
            {
                var gitIntegrationCredentialOptions = sp.GetRequiredService<IOptions<GitIntegrationCredentialOptions>>();
                return new GitRepositoryCredentialOptions(gitIntegrationCredentialOptions.Value.GithubUsername, gitIntegrationCredentialOptions.Value.GithubToken);
            });
    }

    public static IServiceCollection AddZeyaGitIntegration(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton(CreateRepositoryFetcher)
            .AddSingleton<IGitIntegrationService>(sp => new GitIntegrationService(
                sp.GetRequiredService<IOptions<GitEnvironmentOptions>>().Value.CommitAuthor,
                sp.GetRequiredService<IRepositoryFetcher>(),
                sp.GetRequiredService<GitRepositoryCredentialOptions>()))
            .AddSingleton<IGitIntegrationServiceFactory, GitIntegrationServiceFactory>()
            ;
    }

    private static IRepositoryFetcher CreateRepositoryFetcher(IServiceProvider sp)
    {
        IOptions<GitIntegrationCredentialOptions> credentialOptions = sp.GetRequiredService<IOptions<GitIntegrationCredentialOptions>>();
        ILogger<IRepositoryFetcher> logger = sp.GetRequiredService<ILogger<IRepositoryFetcher>>();

        RepositoryFetchOptions repositoryFetchOptions = CreateRepositoryFetchOptions(credentialOptions.Value);
        var repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, logger);
        return new ExceptionHandlerRepositoryFetcherDecorator(repositoryFetcher, logger);
    }

    private static RepositoryFetchOptions CreateRepositoryFetchOptions(GitIntegrationCredentialOptions credentialOptions)
    {
        return credentialOptions.AuthType switch
        {
            GitCredentialType.UserPassword => RepositoryFetchOptions.CreateWithUserPasswordAuth(credentialOptions.GithubUsername, credentialOptions.GithubToken),
            GitCredentialType.HeaderBased => RepositoryFetchOptions.CreateHeaderBasedAuth(credentialOptions.GithubToken),
            _ => throw SwitchDefaultExceptions.OnUnexpectedValue(credentialOptions.AuthType)
        };
    }

    public static IServiceCollection AddZeyaGithubIntegration(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ILocalStoragePathFactory>(sp =>
        {
            var gitEnvironmentOptions = sp.GetRequiredService<IOptions<GitEnvironmentOptions>>();
            return new UseOwnerAndRepoForFolderNameStrategy(gitEnvironmentOptions.Value.CacheDirectoryPath);
        });

        serviceCollection.AddSingleton<IGitHubClient>(sp =>
        {
            var credentials = sp.GetRequiredService<IOptions<GitIntegrationCredentialOptions>>();
            return new GitHubClient(new ProductHeaderValue("Zeya")) { Credentials = new Credentials(credentials.Value.GithubToken) };
        });

        return serviceCollection
            .AddSingleton<IGithubIntegrationService, GithubIntegrationService>()
            .AddSingleton<IGithubIntegrationServiceFactory, GithubIntegrationServiceFactory>()
            .AddSingleton<LocalRepositoryProvider>();
    }

    public static IServiceCollection AddZeyaAdoIntegration(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IAdoIntegrationService, AdoIntegrationService>()
            .AddSingleton<IAdoIntegrationServiceFactory, AdoIntegrationServiceFactory>();
    }
}