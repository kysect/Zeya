using Kysect.Zeya.AdoIntegration.Abstraction;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public class DummyAdoIntegrationService : IAdoIntegrationService
{
    public bool BuildValidationEnabled(string organization, string repository)
    {
        throw new NotImplementedException();
    }
}