using Kysect.Zeya.DependencyManager;
using Kysect.Zeya.Tests.Tools;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.Tests.DependencyManager;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddZeyaRequiredService_DoNotThrowException()
    {
        var serviceProviderOptions = new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true };
        IServiceCollection serviceCollection = new ServiceCollection();
        IConfiguration configuration = TestConfigurationProvider.Create();

        serviceCollection
            .AddSingleton(configuration);

        serviceCollection
            .AddZeyaTestLogging()
            .AddZeyaSqliteDbContext("Database.sql")
            .AddZeyaLocalHandlingService(configuration);

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider(serviceProviderOptions);
    }
}