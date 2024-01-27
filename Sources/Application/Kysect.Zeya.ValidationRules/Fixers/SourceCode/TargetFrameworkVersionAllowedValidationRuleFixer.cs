using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class TargetFrameworkVersionAllowedValidationRuleFixer(
    DotnetSolutionModifierFactory dotnetSolutionModifierFactory,
    IDotnetProjectPropertyAccessor projectPropertyAccessor,
    RepositorySolutionAccessorFactory repositorySolutionAccessorFactory,
    XmlDocumentSyntaxFormatter formatter,
    ILogger logger) : IValidationRuleFixer<TargetFrameworkVersionAllowedValidationRule.Arguments>
{
    public void Fix(TargetFrameworkVersionAllowedValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(clonedRepository);
        string solutionPath = repositorySolutionAccessor.GetSolutionFilePath();
        DotnetSolutionModifier solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);

        HashSet<string> allowedVersion = rule.AllowedVersions.ToHashSet();
        string? expectedTargetVersion = allowedVersion.FirstOrDefault(IsNetVersion);
        if (expectedTargetVersion is null)
        {
            logger.LogError("Cannot update target framework version because no suitable version specified in allowed");
            return;
        }

        foreach (DotnetProjectModifier projectModifier in solutionModifier.Projects)
        {
            // TODO: must get value from projectPropertyAccessor?
            projectPropertyAccessor.ThrowIfNull();
            DotnetProjectProperty dotnetProjectProperty = projectModifier.File.GetProperty("TargetFramework");
            var projectTargetFramework = dotnetProjectProperty.Value;

            if (!IsNetVersion(projectTargetFramework))
            {
                logger.LogWarning("Version {Version} updating is not supported", projectTargetFramework);
                continue;
            }

            if (string.Equals(projectTargetFramework, expectedTargetVersion))
                continue;

            // TODO: return logging
            //logger.LogDebug("Change framework versions from {Current} to {Expected} for project {Project}", projectTargetFramework, expectedTargetVersion, projectModifier.Path);
            projectModifier.File.AddOrUpdateProperty("TargetFramework", expectedTargetVersion);
        }

        // TODO: force somehow saving
        solutionModifier.Save(formatter);
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