using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;

namespace Kysect.Zeya.GithubIntegration;

public class GithubRepositoryAccessorFactory(ILocalStoragePathFactory pathFormatStrategy, IFileSystem fileSystem)
{
    public GithubRepositoryAccessor Create(GithubRepository repository)
    {
        return new GithubRepositoryAccessor(repository, pathFormatStrategy, fileSystem);
    }
}