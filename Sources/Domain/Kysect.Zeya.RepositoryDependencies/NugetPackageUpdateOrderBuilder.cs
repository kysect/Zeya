using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.CommonLib.Graphs;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryDependencies.NuGetPackagesSearchers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kysect.Zeya.RepositoryDependencies;

public class NugetPackageUpdateOrderBuilder
{
    private readonly INuGetPackagesSearcher _nuGetPackagesSearcher;
    private readonly NugetRepositoryClient _nugetRepositoryClient;
    private readonly ILogger _logger;

    public NugetPackageUpdateOrderBuilder(NugetRepositoryClient nugetRepositoryClient, INuGetPackagesSearcher nuGetPackagesSearcher, ILogger<NugetPackageUpdateOrderBuilder> logger)
    {
        _nugetRepositoryClient = nugetRepositoryClient;
        _nuGetPackagesSearcher = nuGetPackagesSearcher;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<GraphLink<string>>> Build(IReadOnlyCollection<ILocalRepository> repositories)
    {
        repositories.ThrowIfNull();

        _logger.LogInformation($"Building dependency graph for repositories: {repositories.ToSingleString(r => r.GetRepositoryName())}");

        Dictionary<string, ILocalRepository> nugetPackageLocations = new Dictionary<string, ILocalRepository>();
        foreach (ILocalRepository localRepository in repositories)
        {
            IReadOnlyCollection<string> solutionProjects = _nuGetPackagesSearcher.GetContainingProjectNames(localRepository);
            foreach (string projectName in solutionProjects)
            {
                if (!await _nugetRepositoryClient.IsExists(projectName))
                {
                    _logger.LogDebug($"Skip project {projectName} from repository {localRepository.GetRepositoryName()}. No associated NuGet packages");
                    continue;
                }

                nugetPackageLocations[projectName] = localRepository;
            }
        }

        List<string> repositoryNames = repositories.Select(r => r.GetRepositoryName()).ToList();
        List<GraphLink<string>> repositoryLinks = new List<GraphLink<string>>();
        foreach ((string nugetPackageName, ILocalRepository localRepository) in nugetPackageLocations)
        {
            IReadOnlyCollection<string> dependencies = await _nugetRepositoryClient.GetDependencies(nugetPackageName);
            foreach (string dependency in dependencies)
            {
                if (!nugetPackageLocations.TryGetValue(dependency, out ILocalRepository? otherRepository))
                {
                    _logger.LogWarning($"Dependency {dependency} of {nugetPackageName} is not found in any repository.");
                    continue;
                }

                if (localRepository.GetRepositoryName() == otherRepository.GetRepositoryName())
                    continue;

                var graphLink = new GraphLink<string>(otherRepository.GetRepositoryName(), localRepository.GetRepositoryName());
                if (!repositoryLinks.Contains(graphLink))
                    repositoryLinks.Add(graphLink);
            }
        }

        return repositoryLinks;
    }
}