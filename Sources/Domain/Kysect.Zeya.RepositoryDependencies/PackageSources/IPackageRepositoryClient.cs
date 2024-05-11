using NuGet.Versioning;
using System.Threading.Tasks;

namespace Kysect.Zeya.RepositoryDependencies.PackageSources;

public interface IPackageRepositoryClient
{
    Task<NuGetVersion?> TryGetLastVersion(string nugetName);
}