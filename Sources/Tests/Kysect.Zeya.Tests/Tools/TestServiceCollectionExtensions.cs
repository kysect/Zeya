using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.DependencyInjection;
using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;
using Kysect.Zeya.AdoIntegration.Abstraction;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.Tests.Tools.Fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

    public static IServiceCollection AddZeyaTestRemoteHostIntegration(this IServiceCollection serviceCollection, IConfiguration configuration, string currentPath)
    {
        configuration.ThrowIfNull();

        RemoteGitHostCredential githubCredential = configuration.GetSection("RemoteHosts").GetSection("Github").GetRequired<RemoteGitHostCredential>();
        RemoteGitHostCredential adoCredentials = configuration.GetSection("RemoteHosts").GetSection("AzureDevOps").GetRequired<RemoteGitHostCredential>();
        //GitEnvironmentOptions gitEnvironmentOptions = configuration.GetSection("GitEnvironmentOptions").GetRequired<GitEnvironmentOptions>();

        serviceCollection
            .AddSingleton<FakeGithubIntegrationService>()
            .AddSingleton<IGithubIntegrationService, FakeGithubIntegrationService>(sp => sp.GetRequiredService<FakeGithubIntegrationService>());

        serviceCollection.AddSingleton<IAdoIntegrationService, DummyAdoIntegrationService>();

        serviceCollection.AddSingleton<ILocalStoragePathFactory>(new FakePathFormatStrategy(currentPath));
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