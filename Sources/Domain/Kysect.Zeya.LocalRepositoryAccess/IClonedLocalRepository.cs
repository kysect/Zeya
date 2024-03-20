namespace Kysect.Zeya.LocalRepositoryAccess;

public interface IClonedLocalRepository : ILocalRepository
{
    void CreatePullRequest(string message, string pullRequestTitle, string branch, string baseBranch);
}