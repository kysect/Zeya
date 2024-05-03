using Kysect.CommonLib.BaseTypes.Extensions;
using System;

namespace Kysect.Zeya.AdoIntegration.Abstraction;

public record struct AdoRepositoryUrl(string OrganizationUrl, string Project, string Repository)
{
    public static AdoRepositoryUrl Parse(string projectUrl)
    {
        projectUrl.ThrowIfNull();

        // TODO: Need to think about better implementation
        string[] parts = projectUrl.Split("/_git/");
        if (parts.Length != 2)
            throw new ArgumentException($"Invalid ADO repository path: {projectUrl}");

        int lastIndexOf = parts[0].LastIndexOf('/');
        if (lastIndexOf == -1)
            throw new ArgumentException($"Invalid ADO repository path: {projectUrl}");

        string organization = parts[0].Substring(0, lastIndexOf);
        string project = parts[0].Substring(lastIndexOf + 1, parts[0].Length - lastIndexOf - 1);
        string repositoryName = parts[1];

        return new AdoRepositoryUrl(organization, project, repositoryName);
    }
}