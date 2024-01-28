using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubReadmeExistsValidationRule : IScenarioStepExecutor<GithubReadmeExistsValidationRule.Arguments>
{
    [ScenarioStep("Github.ReadmeExists")]
    public record Arguments : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Github.ReadmeExists;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();

        if (!repositoryValidationContext.Repository.Exists(ValidationConstants.ReadmeFileName))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                "Readme.md file was not found",
                Arguments.DefaultSeverity);
        }
    }
}