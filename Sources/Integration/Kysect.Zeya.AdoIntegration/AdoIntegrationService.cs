using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.AdoIntegration.Abstraction;
using Microsoft.TeamFoundation.Policy.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json.Linq;

namespace Kysect.Zeya.AdoIntegration;

public class AdoIntegrationService(
    string personalAccessToken,
    string hostUrl)
    : IAdoIntegrationService
{
    public async Task<bool> BuildValidationEnabled(AdoRepositoryUrl repositoryUrl)
    {
        using VssConnection connection = CreateConnection(repositoryUrl);
        GitRepository gitRepository = await GetRepository(connection, repositoryUrl);

        var policyHttpClient = await connection.GetClientAsync<PolicyHttpClient>();
        // https://learn.microsoft.com/en-us/rest/api/azure/devops/policy/configurations/list
        Guid buildPolicyId = Guid.Parse("0609b952-1397-4640-95ec-e00a01b2c241");
        List<PolicyConfiguration> policyConfigurations = await policyHttpClient.GetPolicyConfigurationsAsync(repositoryUrl.Project, policyType: buildPolicyId);

        foreach (PolicyConfiguration policyConfiguration in policyConfigurations)
        {
            Guid repositoryId = GetRepositoryIdFromPolicySettings(policyConfiguration);
            if (repositoryId != gitRepository.Id)
                continue;

            if (policyConfiguration.IsEnabled)
                return true;
        }

        return false;
    }

    private async Task<GitRepository> GetRepository(VssConnection connection, AdoRepositoryUrl repositoryUrl)
    {
        var gitHttpClient = await connection.GetClientAsync<GitHttpClient>();
        GitRepository? gitRepositoryInfo = await gitHttpClient.GetRepositoryAsync(repositoryUrl.Project, repositoryUrl.Repository);
        return gitRepositoryInfo.ThrowIfNull();
    }

    private VssConnection CreateConnection(AdoRepositoryUrl repositoryUrl)
    {
        var organizationUrl = $"{hostUrl}{repositoryUrl.Collection}";
        var baseUrl = new Uri(organizationUrl);
        var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
        return new VssConnection(baseUrl, credentials);
    }

    private Guid GetRepositoryIdFromPolicySettings(PolicyConfiguration policyConfiguration)
    {
        JToken policyScope = policyConfiguration.Settings["scope"].ThrowIfNull();
        string repositoryId = policyScope[0].ThrowIfNull().Value<string>("repositoryId").ThrowIfNull();
        return Guid.Parse(repositoryId);
    }
}