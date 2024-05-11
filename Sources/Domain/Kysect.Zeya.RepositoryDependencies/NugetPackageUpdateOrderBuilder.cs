using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.CommonLib.Graphs;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryDependencies.PackageDataCollecting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kysect.Zeya.RepositoryDependencies;

public record RepositoryDependencyLink(string From, string To, bool IsActual);

public record ActionPlanStep(ILocalRepository Repository, bool FixRequired, IReadOnlyCollection<string> NotUpdatedInternalReferences)
{
    public string ConvertToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Repository {Repository.GetRepositoryName()}:");
        if (FixRequired)
            stringBuilder.AppendLine("Diagnostics fix required;");

        if (!NotUpdatedInternalReferences.IsEmpty())
            stringBuilder.AppendLine($"Not updated internal references: {NotUpdatedInternalReferences.ToSingleString()};");

        return stringBuilder.ToString();
    }
}

public class NugetPackageUpdateOrderBuilder(
    ILogger<NugetPackageUpdateOrderBuilder> logger)
{
    private readonly ILogger _logger = logger;

    public IReadOnlyCollection<RepositoryDependencyLink> CreateDependencyLinks(IReadOnlyCollection<ILocalRepository> repositories, IReadOnlyCollection<SolutionPackageAnalyzerContextItem> solutionPackageAnalyzerContextItems)
    {
        repositories.ThrowIfNull();
        solutionPackageAnalyzerContextItems.ThrowIfNull();

        _logger.LogInformation($"Building dependency graph for repositories: {repositories.ToSingleString(r => r.GetRepositoryName())}");

        Dictionary<string, ILocalRepository> nugetPackageLocations =
            solutionPackageAnalyzerContextItems
            .SelectMany(i => i.DeclaredPackages.Select(p => new { i.Repository, p.Name }))
            .ToDictionary(t => t.Name, t => t.Repository);

        Dictionary<string, string> packageToVersionMapping = solutionPackageAnalyzerContextItems
            .SelectMany(i => i.DeclaredPackages)
            .ToDictionary(p => p.Name, p => p.Version);

        HashSet<RepositoryDependencyLink> repositoryLinks = new();
        foreach (SolutionPackageAnalyzerContextItem solutionPackageAnalyzerContextItem in solutionPackageAnalyzerContextItems)
        {
            foreach (ProjectPackageVersion projectPackageVersion in solutionPackageAnalyzerContextItem.DependencyPackages)
            {
                if (!nugetPackageLocations.TryGetValue(projectPackageVersion.Name, out ILocalRepository? otherRepository))
                {
                    _logger.LogTrace($"Dependency {projectPackageVersion} of {solutionPackageAnalyzerContextItem.Repository.GetRepositoryName()} is not found in any repository.");
                    continue;
                }

                if (solutionPackageAnalyzerContextItem.Repository.GetRepositoryName() == otherRepository.GetRepositoryName())
                    continue;

                if (!packageToVersionMapping.TryGetValue(projectPackageVersion.Name, out string? latestVersion))
                {
                    _logger.LogTrace($"Skip project {projectPackageVersion.Name} skipped. Information in package sources not found.");
                    continue;
                }

                bool isActual = latestVersion == projectPackageVersion.Version;
                // TODO: Possible case when one dependency is actual and another is not
                repositoryLinks.Add(new RepositoryDependencyLink(otherRepository.GetRepositoryName(), solutionPackageAnalyzerContextItem.Repository.GetRepositoryName(), isActual));
            }
        }

        return repositoryLinks;
    }

    public IReadOnlyCollection<ActionPlanStep> CreateFixingActionPlan(IReadOnlyCollection<ILocalRepository> repositories, IReadOnlyCollection<string> repositoriesWithDiagnostics, IReadOnlyCollection<RepositoryDependencyLink> links)
    {
        List<ActionPlanStep> result = [];
        List<ILocalRepository> notVisitedRepositories = repositories.ToList();
        List<RepositoryDependencyLink> currentLinks = links.ToList();

        Dictionary<string, HashSet<string>> notUpdatedInternalReferences = new();
        while (notVisitedRepositories.Any())
        {
            List<GraphLink<string>> graphLinks = currentLinks.Select(l => new GraphLink<string>(l.From, l.To)).ToList();
            List<string> repositoryNames = notVisitedRepositories.Select(r => r.GetRepositoryName()).ToList();

            GraphBuildResult<string, ILocalRepository> graph = GraphBuilder.Build(repositoryNames, graphLinks, GraphValueResolverCreator.Create(notVisitedRepositories, r => r.GetRepositoryName()));

            foreach (GraphNode<string, ILocalRepository> graphRoot in graph.Roots)
            {
                bool currentRepositoryContainsDiagnostic = repositoriesWithDiagnostics.Contains(graphRoot.Id);
                if (!notUpdatedInternalReferences.TryGetValue(graphRoot.Id, out var notUpdatedReferencesForCurrent))
                {
                    notUpdatedReferencesForCurrent = [];
                    notUpdatedInternalReferences[graphRoot.Id] = notUpdatedReferencesForCurrent;
                }

                foreach (GraphNode<string, ILocalRepository> dependantNode in graphRoot.DirectChildren)
                {
                    if (!notUpdatedInternalReferences.TryGetValue(dependantNode.Id, out var notUpdatedReferencesForDependant))
                    {
                        notUpdatedReferencesForDependant = [];
                        notUpdatedInternalReferences[dependantNode.Id] = notUpdatedReferencesForDependant;
                    }

                    if (currentRepositoryContainsDiagnostic)
                        notUpdatedReferencesForDependant.Add(graphRoot.Id);

                    // TODO: do it in better way. Collection can contain duplicates
                    RepositoryDependencyLink dependencyLink = links.First(l => l.From == graphRoot.Id && l.To == dependantNode.Id);
                    if (!dependencyLink.IsActual)
                        notUpdatedReferencesForDependant.Add(graphRoot.Id);
                }

                notVisitedRepositories.RemoveAll(r => r.GetRepositoryName() == graphRoot.Value.GetRepositoryName());
                currentLinks.RemoveAll(l => l.From == graphRoot.Value.GetRepositoryName());

                if (currentRepositoryContainsDiagnostic || !notUpdatedReferencesForCurrent.IsEmpty())
                    result.Add(new ActionPlanStep(graphRoot.Value, currentRepositoryContainsDiagnostic, notUpdatedReferencesForCurrent));
            }
        }

        return result;
    }
}