using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using System.IO.Abstractions;

namespace Kysect.Zeya.GithubIntegration;

public class GithubRepositoryAccessorFactory(ILocalStoragePathFactory pathFormatStrategy, IFileSystem fileSystem) : IClonedRepositoryFactory<ClonedGithubRepositoryAccessor>
{
    public ClonedGithubRepositoryAccessor Create(GithubRepository repository)
    {
        repository.ThrowIfNull();

        string repositoryRootPath = pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(repository.Owner, repository.Name));
        return new ClonedGithubRepositoryAccessor(repository, repositoryRootPath, fileSystem);
    }
}