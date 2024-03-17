using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;

public class VersionInPropFileValidationRule()
    : IScenarioStepExecutor<VersionInPropFileValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.VersionInPropFile")]
    public record Arguments() : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.VersionInPropFile;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        LocalRepositorySolution repositorySolutionAccessor = repositoryValidationContext.Repository.SolutionManager.GetSolution();
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        List<string> projectWithVersions = new List<string>();
        foreach ((string projectName, DotnetCsprojFile? csprojFile) in solutionModifier.Projects)
        {
            if (csprojFile.File.Properties.FindProperty("Version") is not null)
                projectWithVersions.Add(projectName);
        }

        if (projectWithVersions.Any())
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"Project file must be specified in Directory.Build.props file. Projects with version: {projectWithVersions.ToSingleString()}",
                Arguments.DefaultSeverity);
        }
    }
}