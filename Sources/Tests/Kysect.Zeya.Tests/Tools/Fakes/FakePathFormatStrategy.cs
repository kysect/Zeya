using Kysect.GithubUtils.Models;
using Kysect.GithubUtils.Replication.OrganizationsSync.LocalStoragePathFactories;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class FakePathFormatStrategy(string path) : ILocalStoragePathFactory
{
    private readonly string _path = path;

    public string GetPathToRepository(GithubRepository repository)
    {
        return _path;
    }

    public string GetPathToRepositoryWithBranch(GithubRepository repository, string branch)
    {
        return _path;
    }
}