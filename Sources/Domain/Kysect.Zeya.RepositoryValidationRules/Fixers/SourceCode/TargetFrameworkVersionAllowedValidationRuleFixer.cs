using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;

public class TargetFrameworkVersionAllowedValidationRuleFixer(
    XmlDocumentSyntaxFormatter formatter,
    ILogger<TargetFrameworkVersionAllowedValidationRuleFixer> logger) : IValidationRuleFixer<TargetFrameworkVersionAllowedValidationRule.Arguments>
{
    public void Fix(TargetFrameworkVersionAllowedValidationRule.Arguments rule, ILocalRepository localRepository)
    {
        rule.ThrowIfNull();
        localRepository.ThrowIfNull();

        LocalRepositorySolution repositorySolutionAccessor = localRepository.SolutionManager.GetSolution();
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        foreach (KeyValuePair<string, DotnetCsprojFile> projectModifier in solutionModifier.Projects)
        {
            DotnetProjectProperty dotnetProjectProperty = projectModifier.Value.File.Properties.GetProperty("TargetFramework");
            string projectTargetFramework = dotnetProjectProperty.Value;
            string? allowedTargetFrameworkVersion = GetCorrectVersion(rule, projectTargetFramework);

            if (allowedTargetFrameworkVersion is null)
                continue;

            if (string.Equals(projectTargetFramework, allowedTargetFrameworkVersion))
                continue;

            logger.LogDebug("Change framework versions from {Current} to {Expected} for project {Project}", projectTargetFramework, allowedTargetFrameworkVersion, projectModifier.Key);
            projectModifier.Value.File.Properties.SetProperty("TargetFramework", allowedTargetFrameworkVersion);
        }

        // TODO: force somehow saving
        solutionModifier.Save(formatter);
    }

    private string? GetCorrectVersion(TargetFrameworkVersionAllowedValidationRule.Arguments rule, string version)
    {
        if (version.StartsWith("netstandard"))
            return rule.AllowedStandardVersion;

        if (version.StartsWith("net4"))
            return rule.AllowedFrameworkVersion;

        return rule.AllowedCoreVersion;
    }
}