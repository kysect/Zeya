using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryValidation;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class RequiredPackagesAddedValidationRule(RepositorySolutionAccessorFactory repositorySolutionAccessorFactory)
    : IScenarioStepExecutor<RequiredPackagesAddedValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.RequiredPackagesAdded")]
    public record Arguments(IReadOnlyCollection<string> Packages) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.RequiredPackagesAdded;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.Repository);

        if (!repositoryValidationContext.Repository.Exists(repositorySolutionAccessor.GetDirectoryBuildPropsPath()))
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

        foreach (var requestPackage in request.Packages)
        {
            if (!addedPackages.Contains(requestPackage))
            {
                repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                    request.DiagnosticCode,
                    $"Package {requestPackage} is not add to Directory.Build.props.",
                    Arguments.DefaultSeverity);
            }
        }
    }
}