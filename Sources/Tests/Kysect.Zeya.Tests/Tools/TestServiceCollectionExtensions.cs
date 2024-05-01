using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.GithubUtils.Replication.RepositorySync;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Application.Repositories.Github;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kysect.Zeya.Tests.Tools;

public static class TestServiceCollectionExtensions
{
    public static IServiceCollection AddZeyaTestLogging(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddLogging(b =>
            {
                b
                    .AddFilter(null, LogLevel.Trace)
                    .AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = false;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss";
                    });
            });
    }

    public static IServiceCollection AddZeyaTestGitConfiguration(this IServiceCollection serviceCollection)
    {
        var githubIntegrationOptions = new GithubIntegrationOptions()
        {
            CommitAuthor = new GitCommitAuthor()
            {
                GithubUsername = "Name",
                GithubMail = "Name@null.com",
            },
            Credential = new GitIntegrationCredential()
            {
                GithubUsername = "Name",
                GithubToken = "token"
            }
        };

        return serviceCollection
            .AddSingleton<IOptions<GithubIntegrationOptions>>(new OptionsWrapper<GithubIntegrationOptions>(githubIntegrationOptions))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<GithubIntegrationOptions>>().Value.Credential);
    }

    public static IServiceCollection AddZeyaTestGitIntegration(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<IRepositoryFetcher>(sp =>
            {
                ILogger<IRepositoryFetcher> logger = sp.GetRequiredService<ILogger<IRepositoryFetcher>>();
                var repositoryFetchOptions = RepositoryFetchOptions.CreateWithUserPasswordAuth("GithubUsername", "GithubToken");
                return new RepositoryFetcher(repositoryFetchOptions, logger);
            });
    }

    public static IServiceCollection AddZeyaTestGithubIntegration(this IServiceCollection serviceCollection, string currentPath)
    {
        return serviceCollection
            .AddSingleton<ILocalStoragePathFactory>(new FakePathFormatStrategy(currentPath))
            .AddSingleton<FakeGithubIntegrationService>()
            .AddSingleton<IGithubIntegrationService, FakeGithubIntegrationService>(sp => sp.GetRequiredService<FakeGithubIntegrationService>())
            .AddSingleton<IGithubIntegrationServiceFactory, GithubIntegrationServiceFactory>()
            .AddSingleton<LocalRepositoryProvider>();
    }
}