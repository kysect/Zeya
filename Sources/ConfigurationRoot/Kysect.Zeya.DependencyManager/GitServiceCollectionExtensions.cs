using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.DependencyInjection;
using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.Zeya.AdoIntegration;
using Kysect.Zeya.AdoIntegration.Abstraction;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.GitIntegration.Abstraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Kysect.Zeya.DependencyManager;

public static class GitServiceCollectionExtensions
{
    public static IServiceCollection AddZeyaGitIntegration(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        configuration.ThrowIfNull();

        return serviceCollection.AddSingleton<IGitIntegrationServiceFactory, GitIntegrationServiceFactory>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<IGitIntegrationServiceFactory>>();
            var gitEnvironmentOptions = configuration.GetSection("GitEnvironmentOptions").GetRequired<GitEnvironmentOptions>();
            return new GitIntegrationServiceFactory(logger, gitEnvironmentOptions.CommitAuthor);
        });
    }

    public static IServiceCollection AddZeyaRemoteHostIntegration(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        configuration.ThrowIfNull();

        RemoteGitHostCredential githubCredential = configuration.GetSection("RemoteHosts").GetSection("Github").GetRequired<RemoteGitHostCredential>();
        RemoteGitHostCredential adoCredentials = configuration.GetSection("RemoteHosts").GetSection("AzureDevOps").GetRequired<RemoteGitHostCredential>();
        GitEnvironmentOptions gitEnvironmentOptions = configuration.GetSection("GitEnvironmentOptions").GetRequired<GitEnvironmentOptions>();

        serviceCollection
            .AddSingleton<IGitHubClient>(sp => new GitHubClient(new ProductHeaderValue("Zeya")) { Credentials = new Credentials(githubCredential.Token) })
            .AddSingleton<IGithubIntegrationService, GithubIntegrationService>();

        serviceCollection.AddSingleton<IAdoIntegrationService, AdoIntegrationService>(sp => new AdoIntegrationService(adoCredentials.Token));

        serviceCollection.AddSingleton<ILocalStoragePathFactory>(sp => new UseOwnerAndRepoForFolderNameStrategy(gitEnvironmentOptions.CacheDirectoryPath));
        serviceCollection.AddSingleton<LocalRepositoryProvider>();
        serviceCollection.AddSingleton<IRemoteHostIntegrationServiceFactory, RemoteHostIntegrationServiceFactory>(sp =>
        {
            var adoIntegrationService = sp.GetRequiredService<IAdoIntegrationService>();
            var githubIntegrationService = sp.GetRequiredService<IGithubIntegrationService>();
            var gitIntegrationServiceFactory = sp.GetRequiredService<IGitIntegrationServiceFactory>();
            return new RemoteHostIntegrationServiceFactory(adoIntegrationService, githubIntegrationService, gitIntegrationServiceFactory, adoCredentials, githubCredential);
        });

        return serviceCollection;
    }
}