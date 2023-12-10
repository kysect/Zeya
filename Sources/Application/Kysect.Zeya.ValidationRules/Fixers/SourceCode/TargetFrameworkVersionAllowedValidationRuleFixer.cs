using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ManagedDotnetCli;
using Kysect.Zeya.ProjectSystemIntegration.Tools;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class TargetFrameworkVersionAllowedValidationRuleFixer(IFileSystem fileSystem, IDotnetProjectPropertyAccessor projectPropertyAccessor, ILogger logger) : IValidationRuleFixer<TargetFrameworkVersionAllowedValidationRule.Arguments>
{
    public string DiagnosticCode => RuleDescription.SourceCode.TargetFrameworkVersionAllowed;

    public void Fix(TargetFrameworkVersionAllowedValidationRule.Arguments rule, IGithubRepositoryAccessor githubRepository)
    {
        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = DotnetSolutionModifier.Create(solutionPath, fileSystem, logger, new SolutionFileParser(logger));

        HashSet<string> allowedVersion = rule.AllowedVersions.ToHashSet();
        string? targetVersion = allowedVersion.Where(IsNetVersion).FirstOrDefault();
        if (targetVersion is null)
        {
            logger.LogError("Cannot update target framework version because no suitable version specified in allowed");
            return;
        }

        foreach (var projectModifier in solutionModifier.Projects)
        {
            var targetFramework = projectPropertyAccessor.GetTargetFramework(projectModifier.Path);

            if (!IsNetVersion(targetFramework))
            {
                logger.LogWarning("Version {Version} updating is not supported", targetFramework);
                continue;
            }

            var projectPropertyModifier = new ProjectPropertyModifier(projectModifier.Accessor, logger);
            projectPropertyModifier.AddOrUpdateProperty("TargetFramework", targetVersion);
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