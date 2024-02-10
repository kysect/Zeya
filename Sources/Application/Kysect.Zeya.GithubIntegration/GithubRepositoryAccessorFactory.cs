using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Contracts;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using System.IO.Abstractions;

namespace Kysect.Zeya.GithubIntegration;

public class GithubRepositoryAccessorFactory(ILocalStoragePathFactory pathFormatStrategy, IFileSystem fileSystem) : IClonedRepositoryFactory<ClonedGithubRepository>
{
    public ClonedGithubRepository Create(GithubRepositoryName repositoryName)
    {
        repositoryName.ThrowIfNull();

        string repositoryRootPath = pathFormatStrategy.GetPathToRepository(new GithubUtils.Models.GithubRepository(repositoryName.Owner, repositoryName.Name));
        return new ClonedGithubRepository(repositoryName, repositoryRootPath, fileSystem);
    }
}