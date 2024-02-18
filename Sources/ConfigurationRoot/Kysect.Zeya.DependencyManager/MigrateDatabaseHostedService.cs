using Kysect.Zeya.DataAccess.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Kysect.Zeya.DependencyManager;

public class MigrateDatabaseHostedService : IHostedService
{
    private readonly IDbContextFactory<ZeyaDbContext> _dbContextFactory;

    public MigrateDatabaseHostedService(IDbContextFactory<ZeyaDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using ZeyaDbContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await context.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}