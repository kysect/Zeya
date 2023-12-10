using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubReadmeExistsValidationRule : IScenarioStepExecutor<GithubReadmeExistsValidationRule.Arguments>
{
    [ScenarioStep("Github.ReadmeExists")]
    public record Arguments : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Github.ReadmeExists;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments arguments)
    {
        var repositoryValidationContext = context.GetValidationContext();

        if (!repositoryValidationContext.RepositoryAccessor.Exists(ValidationConstants.ReadmeFileName))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                arguments.DiagnosticCode,
                "Readme.md file was not found",
                Arguments.DefaultSeverity);
        }
    }
}