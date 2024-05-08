using Kysect.Zeya.LocalRepositoryAccess;
using System.Collections.Generic;

namespace Kysect.Zeya.RepositoryDependencies.NuGetPackagesSearchers;

public interface INuGetPackagesSearcher
{
    IReadOnlyCollection<string> GetContainingProjectNames(ILocalRepository repository);
}