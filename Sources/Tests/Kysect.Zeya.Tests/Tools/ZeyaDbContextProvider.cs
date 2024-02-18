using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.DependencyManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.Tests.Tools;

public static class ZeyaDbContextProvider
{
    public static IDbContextFactory<ZeyaDbContext> Create()
    {
        return new ServiceCollection()
            .AddZeyaSqliteDbContext(Guid.NewGuid().ToString())
            .BuildServiceProvider()
            .GetRequiredService<IDbContextFactory<ZeyaDbContext>>();
    }
}