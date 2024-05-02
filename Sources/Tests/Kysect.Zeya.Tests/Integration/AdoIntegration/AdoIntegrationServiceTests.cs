using FluentAssertions;
using Kysect.Zeya.AdoIntegration;

namespace Kysect.Zeya.Tests.Integration.AdoIntegration;

public class AdoIntegrationServiceTests
{
    private readonly AdoIntegrationService _adoIntegrationService;

    public AdoIntegrationServiceTests()
    {
        string personalAccessToken = "";
        _adoIntegrationService = new AdoIntegrationService(personalAccessToken);
    }

    [Fact(Skip = "Need token")]
    public void BuildValidationEnabled_ForRepositoryWithEnabledBuildValidation_ReturnTrue()
    {
        bool buildValidationEnabled = _adoIntegrationService.BuildValidationEnabled("https://dev.azure.com/inredikawb", "Fluda");

        buildValidationEnabled.Should().BeTrue();
    }
}