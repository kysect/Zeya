using NuGet.Versioning;
using System.Threading.Tasks;

namespace Kysect.Zeya.RepositoryDependencies.PackageDataCollecting;

public interface IPackageRepositoryClient
{
    Task<NuGetVersion?> TryGetLastVersion(string nugetName);
}