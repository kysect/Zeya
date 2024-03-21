namespace Kysect.Zeya.LocalRepositoryAccess;

public interface IClonedLocalRepository : ILocalRepository
{
    Task CreatePullRequest(string message, string pullRequestTitle, string branch, string baseBranch);
}