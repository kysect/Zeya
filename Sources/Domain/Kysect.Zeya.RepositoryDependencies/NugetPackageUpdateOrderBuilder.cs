using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Parsing;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryDependencies.PackageSources;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kysect.Zeya.RepositoryDependencies;

public record RepositoryDependencyLink(string From, string To, bool IsActual);

public class NugetPackageUpdateOrderBuilder(
    SolutionFileContentParser solutionFileContentParser,
    IPackageRepositoryClient packageRepositoryClient,
    ILogger<NugetPackageUpdateOrderBuilder> logger)
{
    private readonly ILogger _logger = logger;

    public async Task<IReadOnlyCollection<RepositoryDependencyLink>> Build(IReadOnlyCollection<ILocalRepository> repositories)
    {
        repositories.ThrowIfNull();

        _logger.LogInformation($"Building dependency graph for repositories: {repositories.ToSingleString(r => r.GetRepositoryName())}");

        Dictionary<string, ILocalRepository> nugetPackageLocations = GetPackageToRepositoryMapping(repositories);

        HashSet<RepositoryDependencyLink> repositoryLinks = new();
        foreach (ILocalRepository localRepository in repositories)
        {
            DotnetSolutionModifier dotnetSolutionModifier = localRepository.SolutionManager.GetSolution().GetSolutionModifier();
            IReadOnlyCollection<ProjectPackageVersion> solutionPackages = dotnetSolutionModifier.GetOrCreateDirectoryPackagePropsModifier().Versions.GetPackageVersions();
            foreach (ProjectPackageVersion projectPackageVersion in solutionPackages)
            {
                if (!nugetPackageLocations.TryGetValue(projectPackageVersion.Name, out ILocalRepository? otherRepository))
                {
                    _logger.LogTrace($"Dependency {projectPackageVersion} of {localRepository} is not found in any repository.");
                    continue;
                }

                if (localRepository.GetRepositoryName() == otherRepository.GetRepositoryName())
                    continue;

                NuGetVersion? latestVersion = await packageRepositoryClient.TryGetLastVersion(projectPackageVersion.Name);
                if (latestVersion is null)
                {
                    _logger.LogTrace($"Skip project {projectPackageVersion.Name} skipped. Information in package sources not found.");
                    continue;
                }

                // TODO: Possible case when one dependency is actual and another is not
                var currentVersion = NuGetVersion.Parse(projectPackageVersion.Version);
                bool isActual = latestVersion == currentVersion;
                repositoryLinks.Add(new RepositoryDependencyLink(otherRepository.GetRepositoryName(), localRepository.GetRepositoryName(), isActual));
            }
        }

        return repositoryLinks;
    }

    private Dictionary<string, ILocalRepository> GetPackageToRepositoryMapping(IReadOnlyCollection<ILocalRepository> repositories)
    {
        Dictionary<string, ILocalRepository> nugetPackageLocations = new Dictionary<string, ILocalRepository>();
        foreach (ILocalRepository localRepository in repositories)
        {
            IReadOnlyCollection<string> solutionProjects = GetContainingProjectNames(localRepository);
            foreach (string projectName in solutionProjects)
            {
                nugetPackageLocations[projectName] = localRepository;
            }
        }

        return nugetPackageLocations;
    }

    public IReadOnlyCollection<string> GetContainingProjectNames(ILocalRepository repository)
    {
        repository.ThrowIfNull();

        string solutionFilePath = repository.SolutionManager.GetSolutionFilePath();
        string solutionFileContent = repository.FileSystem.ReadAllText(solutionFilePath);
        IReadOnlyCollection<DotnetProjectFileDescriptor> projectFileDescriptors = solutionFileContentParser.ParseSolutionFileContent(solutionFileContent);
        var nugetPackages = projectFileDescriptors.Select(p => p.ProjectName).ToList();

        return nugetPackages;
    }
}