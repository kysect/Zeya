using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.LocalRepositoryAccess.Github;

namespace Kysect.Zeya.Application;

public interface IGithubRepositoryProvider
{
    ILocalRepository InitializeRepository(IValidationPolicyRepository repository);
    LocalGithubRepository GetGithubRepository(string owner, string repository);
    ILocalRepository GetLocalRepository(string path, string solutionSearchMask);
}