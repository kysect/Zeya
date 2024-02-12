using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation.Abstractions;
using Kysect.Zeya.RepositoryValidation.Abstractions.Models;
using Kysect.Zeya.ValidationRules.Abstractions;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class ArtifactsOutputEnabledValidationRule()
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
        LocalRepositorySolution repositorySolutionAccessor = repositoryValidationContext.Repository.SolutionManager.GetSolution();

        if (!repositoryValidationContext.Repository.FileSystem.Exists(repositorySolutionAccessor.GetDirectoryBuildPropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                ValidationRuleMessages.DirectoryBuildPropsFileMissed,
                Arguments.DefaultSeverity);
            return;
        }

        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();
        bool useArtifactsOutput = solutionModifier
            .GetOrCreateDirectoryBuildPropsModifier()
            .ArtifactsOutputEnabled();

        if (!useArtifactsOutput)
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                Arguments.UseArtifactsOutputOptionMustBeTrue,
                Arguments.DefaultSeverity);
        }
    }
}