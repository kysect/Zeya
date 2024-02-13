using Kysect.Zeya.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Kysect.Zeya.DataAccess.EntityFramework;

public class ZeyaDbContext : DbContext
{
    public DbSet<ValidationPolicyEntity> ValidationPolicies { get; set; } = null!;

    public ZeyaDbContext(DbContextOptions<ZeyaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}