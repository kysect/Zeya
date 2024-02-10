using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class FakePathFormatStrategy(string path) : ILocalStoragePathFactory
{
    private readonly string _path = path;

    public string GetPathToRepository(GithubRepository repository)
    {
        return _path;
    }

    public string GetPathToRepositoryWithBranch(GithubRepositoryBranch repositoryBranch)
    {
        return _path;
    }
}