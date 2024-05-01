using Kysect.CommonLib.DependencyInjection;
using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.Tests.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.DependencyManager;

public class ServiceCollectionExtensionsTests
{
    private static readonly FileSystem FileSystem;

    static ServiceCollectionExtensionsTests()
    {
        FileSystem = new FileSystem();
    }

    [Fact]
    public void AddZeyaRequiredService_DoNotThrowException()
    {
        var serviceProviderOptions = new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true };
        IServiceCollection serviceCollection = new ServiceCollection();
        AddTestZeyaConfiguration(serviceCollection);

        serviceCollection
            .AddZeyaTestLogging()
            .AddZeyaSqliteDbContext("Database.sql")
            .AddZeyaLocalHandlingService();

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider(serviceProviderOptions);
    }

    private static IServiceCollection AddTestZeyaConfiguration(IServiceCollection serviceCollection)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(FileSystem.Path.Combine("DependencyManager", "appsettings.json"))
            .Build();

        return serviceCollection
            .AddSingleton(config)
            .AddOptionsWithValidation<GithubIntegrationOptions>("GithubIntegrationOptions");
    }
}