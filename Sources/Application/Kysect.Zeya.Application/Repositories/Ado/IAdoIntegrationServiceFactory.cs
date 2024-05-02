using Kysect.Zeya.AdoIntegration.Abstraction;

namespace Kysect.Zeya.Application.Repositories.Ado;

// TODO: merge this with IGithubIntegrationServiceFactory
public interface IAdoIntegrationServiceFactory
{
    IAdoIntegrationService GetService(AdoValidationPolicyRepository adoValidationPolicyRepository);
}

public class AdoIntegrationServiceFactory(IAdoIntegrationService service) : IAdoIntegrationServiceFactory
{
    public IAdoIntegrationService GetService(AdoValidationPolicyRepository adoValidationPolicyRepository)
    {
        return service;
    }
}