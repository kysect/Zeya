using Kysect.CommonLib.DependencyInjection;
using Kysect.CommonLib.Exceptions;
using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.Zeya.Application.Repositories;
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
        // TODO: Maybe we need to split these settings or rename
        return serviceCollection
            .AddOptionsWithValidation<GithubIntegrationOptions>("GithubIntegrationOptions");
    }

    public static IServiceCollection AddZeyaGitIntegration(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IGitIntegrationService>(sp => new GitIntegrationService(sp.GetRequiredService<IOptions<GithubIntegrationOptions>>().Value.CommitAuthor))
            .AddSingleton(CreateRepositoryFetcher);
    }

    private static IRepositoryFetcher CreateRepositoryFetcher(IServiceProvider sp)
    {
        IOptions<GithubIntegrationOptions> githubOptions = sp.GetRequiredService<IOptions<GithubIntegrationOptions>>();
        ILogger<IRepositoryFetcher> logger = sp.GetRequiredService<ILogger<IRepositoryFetcher>>();

        GithubIntegrationCredential credential = githubOptions.Value.Credential;
        RepositoryFetchOptions repositoryFetchOptions = CreateRepositoryFetchOptions(credential);
        var repositoryFetcher = new RepositoryFetcher(repositoryFetchOptions, logger);
        return new ExceptionHandlerRepositoryFetcherDecorator(repositoryFetcher, logger);
    }

    private static RepositoryFetchOptions CreateRepositoryFetchOptions(GithubIntegrationCredential credential)
    {
        return credential.AuthType switch
        {
            GitCredentialType.UserPassword => RepositoryFetchOptions.CreateWithUserPasswordAuth(credential.GithubUsername, credential.GithubToken),
            GitCredentialType.HeaderBased => RepositoryFetchOptions.CreateHeaderBasedAuth(credential.GithubToken),
            _ => throw SwitchDefaultExceptions.OnUnexpectedValue(credential.AuthType)
        };
    }

    public static IServiceCollection AddZeyaGithubIntegration(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(sp =>
        {
            IOptions<GithubIntegrationOptions> githubOptions = sp.GetRequiredService<IOptions<GithubIntegrationOptions>>();
            return githubOptions.Value.Credential;
        });

        serviceCollection.AddSingleton<ILocalStoragePathFactory>(sp =>
        {
            var githubIntegrationOptions = sp.GetRequiredService<IOptions<GithubIntegrationOptions>>();
            return new UseOwnerAndRepoForFolderNameStrategy(githubIntegrationOptions.Value.CacheDirectoryPath);
        });

        serviceCollection.AddSingleton<IGitHubClient>(sp =>
        {
            var credentials = sp.GetRequiredService<GithubIntegrationCredential>();
            return new GitHubClient(new ProductHeaderValue("Zeya")) { Credentials = new Credentials(credentials.GithubToken) };
        });

        return serviceCollection
            .AddSingleton<IGithubIntegrationService, GithubIntegrationService>()
            .AddSingleton<LocalRepositoryProvider>();
    }
}