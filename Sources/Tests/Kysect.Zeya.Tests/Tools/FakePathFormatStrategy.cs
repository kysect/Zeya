using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.RepositorySync.LocalStoragePathFactories;

namespace Kysect.Zeya.Tests.Tools;

public class FakePathFormatStrategy(string branch) : ILocalStoragePathFactory
{
    private readonly string _branch = branch;

    public string GetPathToRepository(GithubRepository repository)
    {
        return _branch;
    }

    public string GetPathToRepositoryWithBranch(GithubRepositoryBranch repositoryBranch)
    {
        return _branch;
    }
}