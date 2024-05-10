using Kysect.Zeya.AdoIntegration.Abstraction;
using Kysect.Zeya.Application.Repositories.Ado;
using Kysect.Zeya.Application.Repositories.Github;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;

namespace Kysect.Zeya.Application.Repositories;

public interface IRemoteHostIntegrationServiceFactory
{
    IGithubIntegrationService GetGithubService(GithubValidationPolicyRepository githubRepository);
    IAdoIntegrationService GetAdoService(AdoValidationPolicyRepository adoValidationPolicyRepository);
    IGitIntegrationService GetGitService(IValidationPolicyRepository githubRepository);
}