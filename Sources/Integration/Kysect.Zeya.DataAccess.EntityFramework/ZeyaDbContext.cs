﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.DataAccess.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Kysect.Zeya.DataAccess.EntityFramework;

public class ZeyaDbContext : DbContext
{
    public DbSet<ValidationPolicyEntity> ValidationPolicies { get; set; } = null!;
    public DbSet<ValidationPolicyRepository> ValidationPolicyRepositories { get; set; } = null!;
    public DbSet<ValidationPolicyRepositoryDiagnostic> ValidationPolicyRepositoryDiagnostics { get; set; } = null!;

    public ZeyaDbContext(DbContextOptions<ZeyaDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ThrowIfNull();

        modelBuilder.Entity<ValidationPolicyRepositoryDiagnostic>()
            .HasKey(c => new { c.ValidationPolicyRepositoryId, c.RuleId });
    }
}