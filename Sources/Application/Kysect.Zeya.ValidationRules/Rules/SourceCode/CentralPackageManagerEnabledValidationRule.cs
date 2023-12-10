using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ManagedDotnetCli;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class CentralPackageManagerEnabledValidationRule(DotnetCli dotnetCli) : IScenarioStepExecutor<CentralPackageManagerEnabledValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.CentralPackageManagerEnabled")]
    public record Arguments : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.CentralPackageManagerEnabled;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        // TODO: use info from Directory.Package.props instead
        var selectedProjectDefault = repositoryValidationContext
            .RepositoryAccessor
            .GetProjectPaths()
            .FirstOrDefault();

        if (selectedProjectDefault is null)
            return;

        var projectPropertyAccessor = new DotnetProjectPropertyAccessor(selectedProjectDefault, dotnetCli);
        var managePackageVersionsCentrally = projectPropertyAccessor.IsManagePackageVersionsCentrally();
        if (!managePackageVersionsCentrally)
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"The repository does not use Central Package Manager",
                Arguments.DefaultSeverity);
        }
    }
}