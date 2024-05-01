using Kysect.Zeya.GitIntegration.Abstraction;

namespace Kysect.Zeya.Application.Repositories.Git;

public interface IGitIntegrationServiceFactory
{
    IGitIntegrationService GetService(IValidationPolicyRepository githubRepository);
}

public class GitIntegrationServiceFactory(IGitIntegrationService service) : IGitIntegrationServiceFactory
{
    public IGitIntegrationService GetService(IValidationPolicyRepository githubRepository)
    {
        return service;
    }
}