using Kysect.GithubUtils.RepositorySync;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.GithubIntegration;

public class GithubRepositoryAccessor : IGithubRepositoryAccessor
{
    private readonly IPathFormatStrategy _pathFormatStrategy;

    public GithubRepositoryAccessor(GithubRepository repository, IPathFormatStrategy pathFormatStrategy)
    {
        _pathFormatStrategy = pathFormatStrategy;
        Repository = repository;
    }

    public GithubRepository Repository { get; }
    public string GetFullPath()
    {
        return _pathFormatStrategy.GetPathToRepository(Repository.Owner, Repository.Name);
    }
}