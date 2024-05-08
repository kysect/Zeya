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

public class NugetRepositoryClient : IDisposable
{
    private readonly SourceCacheContext _cache;
    private readonly ILogger _logger;

    public NugetRepositoryClient()
    {
        _cache = new SourceCacheContext();
        _logger = NullLogger.Instance;
    }

    public async Task<bool> IsExists(string nugetName)
    {
        CancellationToken cancellationToken = CancellationToken.None;
        FindPackageByIdResource resource = await GetResource(cancellationToken);

        // TODO: implement better validation
        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(nugetName, _cache, _logger, cancellationToken);
        return versions.Any();
    }

    public async Task<IReadOnlyCollection<string>> GetDependencies(string nugetName)
    {
        CancellationToken cancellationToken = CancellationToken.None;
        FindPackageByIdResource resource = await GetResource(cancellationToken);

        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(nugetName, _cache, _logger, cancellationToken);
        if (versions.IsEmpty())
            throw new ArgumentException($"NuGet package {nugetName} versions cannot be resolved.");

        // Last = newest
        NuGetVersion nuGetVersion = versions.Last();
        FindPackageByIdDependencyInfo dependencyInfo = await resource.GetDependencyInfoAsync(nugetName, nuGetVersion, _cache, _logger, cancellationToken);

        List<PackageDependencyGroup> packageDependencyGroups = dependencyInfo.DependencyGroups.ToList();
        // TODO: replace First() with method for selecting correct group
        PackageDependencyGroup packageDependencyGroup = packageDependencyGroups.First();
        return packageDependencyGroup.Packages.Select(p => p.Id).ToList();
    }

    private async Task<FindPackageByIdResource> GetResource(CancellationToken cancellationToken)
    {
        // TODO: need to support multiple sources
        SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        return await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
    }

    public void Dispose()
    {
        _cache.Dispose();
    }
}
