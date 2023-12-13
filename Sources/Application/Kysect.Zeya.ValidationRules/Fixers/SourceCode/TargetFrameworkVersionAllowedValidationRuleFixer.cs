﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ManagedDotnetCli;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class TargetFrameworkVersionAllowedValidationRuleFixer(DotnetSolutionModifierFactory dotnetSolutionModifierFactory, IDotnetProjectPropertyAccessor projectPropertyAccessor, ILogger logger) : IValidationRuleFixer<TargetFrameworkVersionAllowedValidationRule.Arguments>
{
    public void Fix(TargetFrameworkVersionAllowedValidationRule.Arguments rule, IGithubRepositoryAccessor githubRepository)
    {
        rule.ThrowIfNull();
        githubRepository.ThrowIfNull();

        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);

        HashSet<string> allowedVersion = rule.AllowedVersions.ToHashSet();
        string? expectedTargetVersion = allowedVersion.FirstOrDefault(IsNetVersion);
        if (expectedTargetVersion is null)
        {
            logger.LogError("Cannot update target framework version because no suitable version specified in allowed");
            return;
        }

        foreach (var projectModifier in solutionModifier.Projects)
        {
            var projectTargetFramework = projectPropertyAccessor.GetTargetFramework(projectModifier.Path);

            if (!IsNetVersion(projectTargetFramework))
            {
                logger.LogWarning("Version {Version} updating is not supported", projectTargetFramework);
                continue;
            }

            if (string.Equals(projectTargetFramework, expectedTargetVersion))
                continue;

            logger.LogDebug("Change framework versions from {Current} to {Expected} for project {Project}", projectTargetFramework, expectedTargetVersion, projectModifier.Path);
            var projectPropertyModifier = new ProjectPropertyModifier(projectModifier.Accessor, logger);
            projectPropertyModifier.AddOrUpdateProperty("TargetFramework", expectedTargetVersion);
        }

        // TODO: force somehow saving
        solutionModifier.Save();
    }

    // TODO: rework this behavior after changing rule arguments
    private bool IsNetVersion(string value)
    {
        // TODO: do it better
        HashSet<string> supportedVersions = new HashSet<string>()
        {
            "net8.0",
            "net7.0",
            "net6.0",
            "net5.0"
        };

        return supportedVersions.Contains(value);
    }
}