using Kysect.CommonLib.Collections.Extensions;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Kysect.Zeya.RepositoryDependencies.PackageDataCollecting;

public class PackageRepositoryClient : IPackageRepositoryClient, IDisposable
{
    private readonly SourceCacheContext _cache;
    private readonly ILogger _logger;
    private readonly string _packageSourceUrl;

    public PackageRepositoryClient()
    {
        _cache = new SourceCacheContext();
        _logger = NullLogger.Instance;
        // TODO: need to support multiple sources
        _packageSourceUrl = "https://api.nuget.org/v3/index.json";
    }

    public async Task<bool> IsExists(string nugetName)
    {
        CancellationToken cancellationToken = CancellationToken.None;
        FindPackageByIdResource resource = await GetResource(cancellationToken);

        // TODO: implement better validation
        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(nugetName, _cache, _logger, cancellationToken);
        return versions.Any();
    }

    public async Task<NuGetVersion?> TryGetLastVersion(string nugetName)
    {
        CancellationToken cancellationToken = CancellationToken.None;
        FindPackageByIdResource resource = await GetResource(cancellationToken);
        IEnumerable<NuGetVersion> versions = await resource.GetAllVersionsAsync(nugetName, _cache, _logger, cancellationToken);
        return versions.LastOrDefault();
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

        var packageDependencyGroups = dependencyInfo.DependencyGroups.ToList();
        // TODO: replace First() with method for selecting correct group
        PackageDependencyGroup packageDependencyGroup = packageDependencyGroups.First();
        return packageDependencyGroup.Packages.Select(p => p.Id).ToList();
    }

    private async Task<FindPackageByIdResource> GetResource(CancellationToken cancellationToken)
    {
        SourceRepository repository = Repository.Factory.GetCoreV3(_packageSourceUrl);
        return await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
    }

    public void Dispose()
    {
        _cache.Dispose();
    }
}
