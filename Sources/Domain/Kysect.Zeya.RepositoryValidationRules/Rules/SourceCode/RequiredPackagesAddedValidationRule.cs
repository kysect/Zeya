using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using System.ComponentModel.DataAnnotations;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;

public class RequiredPackagesAddedValidationRule()
    : IScenarioStepExecutor<RequiredPackagesAddedValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.RequiredPackagesAdded")]
    public record Arguments(
        [property: Required] IReadOnlyCollection<ProjectPackageVersion> Packages) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.RequiredPackagesAdded;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();
        LocalRepositorySolution repositorySolutionAccessor = repositoryValidationContext.Repository.SolutionManager.GetSolution();

        if (!repositoryValidationContext.Repository.FileSystem.Exists(repositorySolutionAccessor.GetDirectoryBuildPropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                ValidationRuleMessages.DirectoryBuildPropsFileMissed,
                Arguments.DefaultSeverity);
            return;
        }

        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();
        DirectoryBuildPropsFile directoryBuildPropsFile = solutionModifier.GetOrCreateDirectoryBuildPropsModifier();

        var addedPackages = directoryBuildPropsFile
            .File
            .GetItems("PackageReference")
            .Select(r => r.Include)
            .ToHashSet();

        foreach (ProjectPackageVersion requestPackage in request.Packages)
        {
            if (!addedPackages.Contains(requestPackage.Name))
            {
                repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                    request.DiagnosticCode,
                    $"Package {requestPackage.Name} is not add to Directory.Build.props.",
                    Arguments.DefaultSeverity);
            }
        }
    }
}