using NuGet.Versioning;

namespace Kysect.Zeya.RepositoryDependencies.PackageDataCollecting;

public interface IPackageRepositoryClient
{
    Task<NuGetVersion?> TryGetLastVersion(string nugetName);
}