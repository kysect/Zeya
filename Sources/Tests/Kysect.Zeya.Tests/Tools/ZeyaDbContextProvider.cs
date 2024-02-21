using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.DependencyManager;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.Tests.Tools;

public static class ZeyaDbContextProvider
{
    public static ZeyaDbContext CreateContext()
    {
        return new ServiceCollection()
            .AddZeyaSqliteDbContext(Guid.NewGuid().ToString())
            .BuildServiceProvider()
            .GetRequiredService<ZeyaDbContext>();
    }
}