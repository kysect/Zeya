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
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile(FileSystem.Path.Combine("DependencyManager", "appsettings.json"))
            .Build();

        serviceCollection
            .AddSingleton(config)
            .AddZeyaGitConfiguration();

        serviceCollection
            .AddZeyaTestLogging()
            .AddZeyaSqliteDbContext("Database.sql")
            .AddZeyaLocalHandlingService(config);

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider(serviceProviderOptions);
    }
}