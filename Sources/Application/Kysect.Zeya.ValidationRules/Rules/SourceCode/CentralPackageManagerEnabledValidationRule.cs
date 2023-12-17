using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ManagedDotnetCli;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class CentralPackageManagerEnabledValidationRule(IDotnetProjectPropertyAccessor projectPropertyAccessor, RepositorySolutionAccessorFactory repositorySolutionAccessorFactory)
    : IScenarioStepExecutor<CentralPackageManagerEnabledValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.CentralPackageManagerEnabled")]
    public record Arguments : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.CentralPackageManagerEnabled;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();

        // TODO: use info from Directory.Package.props instead
        var repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.RepositoryAccessor);

        var selectedProjectDefault = repositorySolutionAccessor
            .GetProjectPaths()
            .FirstOrDefault();

        if (selectedProjectDefault is null)
            return;

        var managePackageVersionsCentrally = projectPropertyAccessor.IsManagePackageVersionsCentrally(selectedProjectDefault);
        if (!managePackageVersionsCentrally)
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"The repository does not use Central Package Manager",
                Arguments.DefaultSeverity);
        }
    }
}