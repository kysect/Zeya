using Kysect.Zeya.GithubIntegration.Abstraction;

namespace Kysect.Zeya.Application.Repositories.Github;

public interface IGithubIntegrationServiceFactory
{
    IGithubIntegrationService GetService(GithubValidationPolicyRepository githubRepository);
}

public class GithubIntegrationServiceFactory(IGithubIntegrationService service) : IGithubIntegrationServiceFactory
{
    public IGithubIntegrationService GetService(GithubValidationPolicyRepository githubRepository)
    {
        return service;
    }
}