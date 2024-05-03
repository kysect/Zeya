using Kysect.Zeya.AdoIntegration.Abstraction;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class DummyAdoIntegrationService : IAdoIntegrationService
{
    public Task<bool> BuildValidationEnabled(AdoRepositoryUrl repositoryUrl)
    {
        throw new NotImplementedException();
    }
}