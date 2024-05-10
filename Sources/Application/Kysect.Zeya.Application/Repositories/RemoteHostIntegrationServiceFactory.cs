using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.AdoIntegration.Abstraction;
using Kysect.Zeya.Application.Repositories.Ado;
using Kysect.Zeya.Application.Repositories.Github;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GitIntegration.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace Kysect.Zeya.Application.Repositories;

public class RemoteHostIntegrationServiceFactory(
    IAdoIntegrationService adoIntegrationService,
    IGithubIntegrationService githubIntegrationService,
    IGitIntegrationServiceFactory gitIntegrationServiceFactory,
    [FromKeyedServices("AdoCredentials")] RemoteGitHostCredential adoCredentials,
    [FromKeyedServices("GithubCredential")] RemoteGitHostCredential githubCredential
) : IRemoteHostIntegrationServiceFactory
{
    public IGithubIntegrationService GetGithubService(GithubValidationPolicyRepository githubRepository)
    {
        return githubIntegrationService;
    }

    public IAdoIntegrationService GetAdoService(AdoValidationPolicyRepository adoValidationPolicyRepository)
    {
        return adoIntegrationService;
    }

    public IGitIntegrationService GetGitService(IValidationPolicyRepository githubRepository)
    {
        githubRepository.ThrowIfNull();

        // TODO: Add support for other types of repositories
        if (githubRepository.Type == ValidationPolicyRepositoryType.Ado)
            return gitIntegrationServiceFactory.CreateGitIntegration(adoCredentials);
        return gitIntegrationServiceFactory.CreateGitIntegration(githubCredential);
    }
}