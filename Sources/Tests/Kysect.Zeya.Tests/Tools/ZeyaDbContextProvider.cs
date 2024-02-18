using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.DependencyManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.Tests.Tools;

public static class ZeyaDbContextProvider
{
    public static IDbContextFactory<ZeyaDbContext> Create()
    {
        ServiceProvider serviceProvider = new ServiceCollection()
            .AddZeyaSqliteDbContext()
            .BuildServiceProvider();

        IDbContextFactory<ZeyaDbContext> dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<ZeyaDbContext>>();
        return dbContextFactory;
    }
}