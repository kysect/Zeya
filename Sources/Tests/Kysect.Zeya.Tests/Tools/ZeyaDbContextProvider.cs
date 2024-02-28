using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.DependencyManager;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.Tests.Tools;

public static class ZeyaDbContextProvider
{
    public static ZeyaDbContext CreateContext()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddZeyaSqliteDbContext(Guid.NewGuid().ToString())
            .BuildServiceProvider();

        ServiceInitialize.InitializeDatabase(serviceProvider).Wait();
        return serviceProvider.GetRequiredService<ZeyaDbContext>();
    }
}