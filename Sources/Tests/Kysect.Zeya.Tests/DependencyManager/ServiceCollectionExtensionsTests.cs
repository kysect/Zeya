using Kysect.Zeya.DependencyManager;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.Tests.DependencyManager;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddZeyaRequiredService_DoNotThrowException()
    {
        var serviceProviderOptions = new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true };
        IServiceCollection serviceCollection = new ServiceCollection()
            .AddZeyaRequiredService();

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider(serviceProviderOptions);
    }
}