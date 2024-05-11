using Kysect.Zeya.Application.DatabaseQueries;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryDependencies;
using Kysect.Zeya.RepositoryDependencies.Visualization;
using Microsoft.EntityFrameworkCore;

namespace Kysect.Zeya.Application.LocalHandling;

public class RepositoryDependenciesService(
    ValidationPolicyRepositoryFactory repositoryFactory,
    LocalRepositoryProvider localRepositoryProvider,
    NugetPackageUpdateOrderBuilder nugetPackageUpdateOrderBuilder,
    ValidationPolicyDatabaseQueries databaseQueries,
    ZeyaDbContext context) : IRepositoryDependenciesService
{
    public async Task<string> GetRepositoryDependenciesTree(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepository> repositories = await context
            .ValidationPolicyRepositories
            .Where(r => r.ValidationPolicyId == policyId)
            .ToListAsync();

        List<ILocalRepository> localRepositories = repositories
            .Select(repositoryFactory.Create)
            .Select(localRepositoryProvider.InitializeRepository)
            .ToList();

        IReadOnlyCollection<string> repositoriesWithDiagnostics = await GetRepositoriesWithDiagnostics(policyId);
        IReadOnlyCollection<RepositoryDependencyLink> graphLinks = await nugetPackageUpdateOrderBuilder.Build(localRepositories);
        return new PlantUmlRepositoryDependencyVisualization().ConvertToString(graphLinks, repositoriesWithDiagnostics);
    }

    private async Task<IReadOnlyCollection<string>> GetRepositoriesWithDiagnostics(Guid policyId)
    {
        IReadOnlyCollection<RepositoryDiagnosticTableRow> repositoriesDiagnostics = await databaseQueries.GetDiagnosticsTable(policyId);
        return repositoriesDiagnostics
            .Where(r => r.Diagnostics.Any())
            .Select(r => r.RepositoryName)
            .ToList();
    }
}