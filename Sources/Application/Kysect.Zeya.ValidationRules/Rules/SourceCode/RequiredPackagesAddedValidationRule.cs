using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;

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
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                ValidationRuleMessages.DirectoryBuildPropsFileMissed,
                Arguments.DefaultSeverity);
            return;
        }

        var directoryBuildProps = repositoryValidationContext.Repository.ReadAllText(repositorySolutionAccessor.GetDirectoryBuildPropsPath());
        var directoryBuildPropsFile = new DirectoryBuildPropsFile(DotnetProjectFile.Create(directoryBuildProps));

        var addedPackages = directoryBuildPropsFile
            .File
            .GetItems("PackageReference")
            .Select(r => r.Include)
            .ToHashSet();

        foreach (var requestPackage in request.Packages)
        {
            if (!addedPackages.Contains(requestPackage))
            {
                repositoryValidationContext.DiagnosticCollector.Add(
                    request.DiagnosticCode,
                    $"Package {requestPackage} is not add to Directory.Build.props.",
                    Arguments.DefaultSeverity);
            }
        }
    }
}