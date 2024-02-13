using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.DependencyManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.Tests.Tools;

public static class ZeyaDbContextProvider
{
    public static IDbContextFactory<ZeyaDbContext> Create()
    {
        var serviceProviderOptions = new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true };
        IServiceCollection serviceCollection = new ServiceCollection();
        serviceCollection
            .AddZeyaDbContext();

        IDbContextFactory<ZeyaDbContext> dbContextFactory = serviceCollection.BuildServiceProvider().GetRequiredService<IDbContextFactory<ZeyaDbContext>>();
        return dbContextFactory;
    }
}