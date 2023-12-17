using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;

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
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.RepositoryAccessor);

        if (!repositoryValidationContext.RepositoryAccessor.Exists(repositorySolutionAccessor.GetDirectoryBuildPropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                "Directory.Build.props file is not exists.",
                DirectoryBuildPropsContainsRequiredFields.DefaultSeverity);
            return;
        }

        var directoryBuildProps = repositoryValidationContext.RepositoryAccessor.ReadAllText(repositorySolutionAccessor.GetDirectoryBuildPropsPath());
        var parser = new DirectoryBuildPropsParser();
        var addedPackages = parser.GetListOfPackageReference(directoryBuildProps).ToHashSet();

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