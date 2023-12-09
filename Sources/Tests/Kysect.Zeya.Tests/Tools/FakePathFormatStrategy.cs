using Kysect.GithubUtils.RepositorySync;

namespace Kysect.Zeya.Tests.Tools;

public class FakePathFormatStrategy(string branch) : IPathFormatStrategy
{
    private readonly string _branch = branch;

    public string GetPathToRepository(string organization, string repository)
    {
        return _branch;
    }

    public string GetPathToRepositoryWithBranch(string organization, string branch, string repository)
    {
        return _branch;
    }
}