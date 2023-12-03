using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class ArtifactsOutputEnabledValidationRule : IScenarioStepExecutor<ArtifactsOutputEnabledValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.ArtifactsOutputEnabled")]
    public record Arguments : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.SourceCode.ArtifactsOutputEnables;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        if (!repositoryValidationContext.RepositoryAccessor.Exists(ValidationConstants.DirectoryPackagePropsPath))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                "Directory.Build.props file is not exists and does not contains UseArtifactsOutput option",
                Arguments.DefaultSeverity);
        }

        var directoryBuildPropsContent = repositoryValidationContext.RepositoryAccessor.ReadFile(ValidationConstants.DirectoryPackagePropsPath);
        var directoryBuildPropsParser = new DirectoryBuildPropsParser();
        Dictionary<string, string> buildPropsValues = directoryBuildPropsParser.Parse(directoryBuildPropsContent);

        if (!buildPropsValues.ContainsKey("UseArtifactsOutput"))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                "Directory.Build.props does not contains UseArtifactsOutput option.",
                Arguments.DefaultSeverity);
        }
    }
}