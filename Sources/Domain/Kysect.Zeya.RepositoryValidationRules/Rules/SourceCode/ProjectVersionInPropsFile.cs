using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.RepositoryValidation;

namespace Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;

public class ProjectVersionInPropsFile()
    : IScenarioStepExecutor<ProjectVersionInPropsFile.Arguments>
{
    [ScenarioStep("SourceCode.ProjectVersionInPropsFile")]
    public record Arguments() : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.RequiredPackagesAdded;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        throw new NotImplementedException();
    }
}