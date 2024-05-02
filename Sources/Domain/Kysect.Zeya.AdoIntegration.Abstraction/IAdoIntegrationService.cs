namespace Kysect.Zeya.AdoIntegration.Abstraction;

public interface IAdoIntegrationService
{
    bool BuildValidationEnabled(string organization, string repository);
}