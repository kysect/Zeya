using Kysect.DotnetSlnParser.Parsers;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ManagedDotnetCli;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class CentralPackageManagerEnabledValidationRule(IDotnetProjectPropertyAccessor projectPropertyAccessor, SolutionFileParser solutionFileParser)
    : IScenarioStepExecutor<CentralPackageManagerEnabledValidationRule.Arguments>
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
        var repositorySolutionAccessor = new RepositorySolutionAccessor(repositoryValidationContext.RepositoryAccessor, solutionFileParser);

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