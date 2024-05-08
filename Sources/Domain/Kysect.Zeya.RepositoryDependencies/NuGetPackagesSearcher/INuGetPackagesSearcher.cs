using Kysect.Zeya.LocalRepositoryAccess;
using System.Collections.Generic;

namespace Kysect.Zeya.RepositoryDependencies.NuGetPackagesSearcher;

public interface INuGetPackagesSearcher
{
    IReadOnlyCollection<string> GetContainingProjectNames(ILocalRepository repository);
}