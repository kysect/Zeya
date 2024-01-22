using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class ArtifactsOutputEnabledValidationRule(RepositorySolutionAccessorFactory repositorySolutionAccessorFactory)
    : IScenarioStepExecutor<ArtifactsOutputEnabledValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.ArtifactsOutputEnabled")]
    public record Arguments : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.SourceCode.ArtifactsOutputEnables;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
        public const string UseArtifactsOutputOptionMissed = "Directory.Build.props does not contains UseArtifactsOutput option";
        public const string UseArtifactsOutputOptionMustBeTrue = "Directory.Build.props option UseArtifactsOutput must be true";
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        RepositoryValidationContext repositoryValidationContext = context.GetValidationContext();
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.Repository);

        if (!repositoryValidationContext.Repository.Exists(repositorySolutionAccessor.GetDirectoryBuildPropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                ValidationRuleMessages.DirectoryBuildPropsFileMissed,
                Arguments.DefaultSeverity);
            return;
        }

        string directoryBuildPropsContent = repositoryValidationContext.Repository.ReadAllText(repositorySolutionAccessor.GetDirectoryBuildPropsPath());
        var directoryBuildPropsParser = new DirectoryBuildPropsParser();
        Dictionary<string, string> buildPropsValues = directoryBuildPropsParser.Parse(directoryBuildPropsContent);

        // TODO: verify that value set to true?
        if (!buildPropsValues.TryGetValue("UseArtifactsOutput", out string? value))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                Arguments.UseArtifactsOutputOptionMissed,
                Arguments.DefaultSeverity);
            return;
        }

        if (!string.Equals(value, "true", StringComparison.InvariantCultureIgnoreCase))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                Arguments.UseArtifactsOutputOptionMustBeTrue,
                Arguments.DefaultSeverity);
        }
    }
}