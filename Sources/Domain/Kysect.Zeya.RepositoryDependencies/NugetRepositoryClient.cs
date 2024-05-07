using Kysect.CommonLib.Collections.Extensions;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kysect.Zeya.RepositoryDependencies;

public class NugetRepositoryClient
{
    public async Task<IReadOnlyCollection<string>> GetDependencies(string nugetName)
    {
        ILogger logger = NullLogger.Instance;
        CancellationToken cancellationToken = CancellationToken.None;

        using SourceCacheContext cache = new SourceCacheContext();
        // TODO: need to support multiple sources
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(nugetName, cache, logger, cancellationToken);

        // TODO: need to introduce method for checking if exists
        if (versions.IsEmpty())
            throw new ArgumentException($"NuGet package {nugetName} is not found in NuGet repository");

        // Last = newest
        NuGetVersion nuGetVersion = versions.Last();
        FindPackageByIdDependencyInfo dependencyInfo = await resource.GetDependencyInfoAsync(nugetName, nuGetVersion, cache, logger, cancellationToken);

        List<PackageDependencyGroup> packageDependencyGroups = dependencyInfo.DependencyGroups.ToList();
        // TODO: replace First() with method for selecting correct group
        PackageDependencyGroup packageDependencyGroup = packageDependencyGroups.First();
        return packageDependencyGroup.Packages.Select(p => p.Id).ToList();
    }
}
