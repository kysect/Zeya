using FluentAssertions;
using Kysect.Zeya.AdoIntegration;
using Kysect.Zeya.AdoIntegration.Abstraction;

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
    public async Task BuildValidationEnabled_ForRepositoryWithEnabledBuildValidation_ReturnTrue()
    {
        var repositoryUrlParts = AdoRepositoryUrl.Parse("https://dev.azure.com/inredikawb/Fluda/_git/Fluda");

        bool buildValidationEnabled = await _adoIntegrationService.BuildValidationEnabled(repositoryUrlParts);

        buildValidationEnabled.Should().BeTrue();
    }
}