using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;

public class VersionInPropFileValidationRuleFixer(ILogger<VersionInPropFileValidationRuleFixer> logger)
    : IValidationRuleFixer<VersionInPropFileValidationRule.Arguments>
{
    public void Fix(VersionInPropFileValidationRule.Arguments rule, ILocalRepository localRepository)
    {
        localRepository.ThrowIfNull();

        LocalRepositorySolution localRepositorySolution = localRepository.SolutionManager.GetSolution();
        DotnetSolutionModifier solutionModifier = localRepositorySolution.GetSolutionModifier();

        HashSet<string> versionValues = [];
        foreach ((string? projectPath, DotnetCsprojFile? projectFile) in solutionModifier.Projects)
        {
            DotnetProjectProperty? versionProperty = projectFile.File.Properties.FindProperty("Version");
            if (versionProperty is not null)
            {
                versionValues.Add(versionProperty.Value.Value);
                projectFile.File.Properties.RemoveProperty("Version");
            }
        }

        if (versionValues.IsEmpty())
            return;

        if (versionValues.Count > 1)
        {
            logger.LogError("Solution contains multiple version properties in .csproj files: {Versions}", versionValues.ToSingleString());
            return;
        }

        string versionValue = versionValues.Single();
        solutionModifier.GetOrCreateDirectoryBuildPropsModifier().File.Properties.SetProperty("Version", versionValue);
        solutionModifier.Save();
    }
}