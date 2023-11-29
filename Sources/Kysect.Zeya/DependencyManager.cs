using Kysect.CommonLib.DependencyInjection;
using Kysect.GithubUtils.RepositorySync;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.GithubIntegration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya;

public class DependencyManager
{
    public IServiceProvider BuildServiceProvider()
    {
        var serviceCollection = new ServiceCollection();

        ILogger logger = PredefinedLogger.CreateConsoleLogger("Zeya");
        var githubIntegrationOptions = new GithubIntegrationOptions()
        {
        };

        var pathFormatStrategy = new UseOwnerAndRepoForFolderNameStrategy(githubIntegrationOptions.CacheDirectoryPath);

        serviceCollection.AddSingleton(logger);
        serviceCollection.AddSingleton(githubIntegrationOptions);
        serviceCollection.AddSingleton<IPathFormatStrategy>(pathFormatStrategy);
        serviceCollection.AddSingleton<IGithubRepositoryProvider, GithubRepositoryProvider>();
        serviceCollection.AddSingleton<IGithubIntegrationService, GithubIntegrationService>();
        serviceCollection.AddSingleton<IRepositoryValidationReporter, LoggerRepositoryValidationReporter>();
        serviceCollection.AddSingleton<DemoScenario>();

        return serviceCollection.BuildServiceProvider();
    }
}