using Kysect.Zeya.Application.DatabaseQueries;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.Client.Abstractions;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryDependencies;
using Kysect.Zeya.RepositoryDependencies.PackageDataCollecting;
using Kysect.Zeya.RepositoryDependencies.Visualization;
using Microsoft.EntityFrameworkCore;

namespace Kysect.Zeya.Application.LocalHandling;

public class RepositoryDependenciesService(
    ValidationPolicyRepositoryFactory repositoryFactory,
    LocalRepositoryProvider localRepositoryProvider,
    SolutionPackageDataCollector solutionPackageDataCollector,
    NugetPackageUpdateOrderBuilder nugetPackageUpdateOrderBuilder,
    ValidationPolicyDatabaseQueries databaseQueries,
    ZeyaDbContext context) : IRepositoryDependenciesService
{
    public async Task<string> GetRepositoryDependenciesTree(Guid policyId)
    {
        List<ILocalRepository> localRepositories = await GetLocalRepositoriesForPolicy(policyId);
        IReadOnlyCollection<string> repositoriesWithDiagnostics = await GetRepositoriesWithDiagnostics(policyId);
        IReadOnlyCollection<SolutionPackageAnalyzerContextItem> solutionPackageAnalyzerContextItems = await solutionPackageDataCollector.Collect(localRepositories);
        IReadOnlyCollection<RepositoryDependencyLink> graphLinks = nugetPackageUpdateOrderBuilder.CreateDependencyLinks(localRepositories, solutionPackageAnalyzerContextItems);
        return new PlantUmlRepositoryDependencyVisualization().ConvertToString(graphLinks, repositoriesWithDiagnostics);
    }

    public async Task<IReadOnlyCollection<FixingActionPlanRow>> GetFixingActionPlan(Guid policyId)
    {
        List<ILocalRepository> localRepositories = await GetLocalRepositoriesForPolicy(policyId);
        IReadOnlyCollection<string> repositoriesWithDiagnostics = await GetRepositoriesWithDiagnostics(policyId);
        IReadOnlyCollection<SolutionPackageAnalyzerContextItem> solutionPackageAnalyzerContextItems = await solutionPackageDataCollector.Collect(localRepositories);
        IReadOnlyCollection<RepositoryDependencyLink> graphLinks = nugetPackageUpdateOrderBuilder.CreateDependencyLinks(localRepositories, solutionPackageAnalyzerContextItems);
        IReadOnlyCollection<ActionPlanStep> actionPlanSteps = nugetPackageUpdateOrderBuilder.CreateFixingActionPlan(localRepositories, repositoriesWithDiagnostics, graphLinks);

        // TODO: return correct ID
        List<FixingActionPlanRow> actionPlanRows = actionPlanSteps
            .Select(s => new FixingActionPlanRow(Guid.Empty, s.Repository.GetRepositoryName(), s.ConvertToString()))
            .ToList();

        return actionPlanRows;
    }

    private async Task<List<ILocalRepository>> GetLocalRepositoriesForPolicy(Guid policyId)
    {
        IReadOnlyCollection<ValidationPolicyRepository> repositories = await context
            .ValidationPolicyRepositories
            .Where(r => r.ValidationPolicyId == policyId)
            .ToListAsync();

        List<ILocalRepository> localRepositories = repositories
            .Select(repositoryFactory.Create)
            .Select(localRepositoryProvider.InitializeRepository)
            .ToList();
        return localRepositories;
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