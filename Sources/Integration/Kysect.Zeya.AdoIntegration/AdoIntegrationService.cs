using Kysect.Zeya.AdoIntegration.Abstraction;
using Microsoft.TeamFoundation.Policy.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;

namespace Kysect.Zeya.AdoIntegration;

public class AdoIntegrationService(
    string personalAccessToken)
    : IAdoIntegrationService
{
    public bool BuildValidationEnabled(string organization, string repository)
    {
        var baseUrl = new Uri(organization);
        var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
        using var connection = new VssConnection(baseUrl, credentials);

        var policyHttpClient = connection.GetClient<PolicyHttpClient>();
        List<PolicyConfiguration> policyConfigurations = policyHttpClient.GetPolicyConfigurationsAsync(repository).Result;

        foreach (PolicyConfiguration policyConfiguration in policyConfigurations)
        {
            if (policyConfiguration.Type.DisplayName != "Build")
                continue;

            if (policyConfiguration.IsEnabled)
                return true;
        }

        return false;
    }
}