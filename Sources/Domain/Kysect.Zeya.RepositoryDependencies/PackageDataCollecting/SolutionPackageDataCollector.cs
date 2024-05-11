using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kysect.Zeya.RepositoryDependencies.PackageDataCollecting;

public record SolutionPackageAnalyzerContextItem(
    ILocalRepository Repository,
    IReadOnlyCollection<ProjectPackageVersion> DeclaredPackages,
    IReadOnlyCollection<ProjectPackageVersion> DependencyPackages);

public class SolutionPackageDataCollector(
    SolutionFileContentParser solutionFileContentParser,
    IPackageRepositoryClient packageRepositoryClient,
    ILogger<SolutionPackageDataCollector> logger)
{
    public async Task<IReadOnlyCollection<SolutionPackageAnalyzerContextItem>> Collect(IReadOnlyCollection<ILocalRepository> repositories)
    {
        repositories.ThrowIfNull();

        List<SolutionPackageAnalyzerContextItem> result = [];
        logger.LogInformation($"Collecting dependency info for repositories: {repositories.ToSingleString(r => r.GetRepositoryName())}");

        foreach (ILocalRepository localRepository in repositories)
        {
            IReadOnlyCollection<ProjectPackageVersion> declaredPackages = await GetDeclaredPackages(localRepository);
            IReadOnlyCollection<ProjectPackageVersion> dependencyPackages = await GetDependencyPackages(localRepository);
            result.Add(new SolutionPackageAnalyzerContextItem(localRepository, declaredPackages, dependencyPackages));
        }

        return result;
    }

    private async Task<IReadOnlyCollection<ProjectPackageVersion>> GetDeclaredPackages(ILocalRepository localRepository)
    {
        List<ProjectPackageVersion> declaredPackages = [];
        IReadOnlyCollection<string> solutionProjects = GetContainingProjectNames(localRepository);
        foreach (string solutionProject in solutionProjects)
        {
            NuGetVersion? declaredPackage = await packageRepositoryClient.TryGetLastVersion(solutionProject);
            if (declaredPackage is not null)
                declaredPackages.Add(new ProjectPackageVersion(solutionProject, declaredPackage.ToFullString()));
        }

        return declaredPackages;
    }

    private IReadOnlyCollection<string> GetContainingProjectNames(ILocalRepository repository)
    {
        repository.ThrowIfNull();

        string solutionFilePath = repository.SolutionManager.GetSolutionFilePath();
        string solutionFileContent = repository.FileSystem.ReadAllText(solutionFilePath);
        IReadOnlyCollection<DotnetProjectFileDescriptor> projectFileDescriptors = solutionFileContentParser.ParseSolutionFileContent(solutionFileContent);
        var nugetPackages = projectFileDescriptors.Select(p => p.ProjectName).ToList();

        return nugetPackages;
    }

    private Task<IReadOnlyCollection<ProjectPackageVersion>> GetDependencyPackages(ILocalRepository localRepository)
    {
        DotnetSolutionModifier dotnetSolutionModifier = localRepository.SolutionManager.GetSolution().GetSolutionModifier();
        IReadOnlyCollection<ProjectPackageVersion> solutionPackages = dotnetSolutionModifier.GetOrCreateDirectoryPackagePropsModifier().Versions.GetPackageVersions();
        return Task.FromResult(solutionPackages);
    }
}