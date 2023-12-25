using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;

namespace Kysect.Zeya.GithubIntegration;

public class GithubRepositoryAccessorFactory(ILocalStoragePathFactory pathFormatStrategy, IFileSystem fileSystem)
{
    public ClonedRepository Create(GithubRepository repository)
    {
        repository.ThrowIfNull();

        string repositoryRootPath = pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
        return new ClonedRepository(repositoryRootPath, fileSystem);
    }
}