using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Collections.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.ProjectSystemIntegration;

namespace Kysect.Zeya.ValidationRules.Rules.Nuget;

public class NugetMetadataHaveCorrectValueValidationRule(RepositorySolutionAccessorFactory repositorySolutionAccessorFactory)
    : IScenarioStepExecutor<NugetMetadataHaveCorrectValueValidationRule.Arguments>
{
    [ScenarioStep("Nuget.MetadataHaveCorrectValue")]
    public record Arguments(Dictionary<string, string> RequiredKeyValues) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Nuget.MetadataHaveCorrectValue;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();
        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(repositoryValidationContext.Repository);

        if (!repositoryValidationContext.Repository.Exists(repositorySolutionAccessor.GetDirectoryBuildPropsPath()))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                ValidationRuleMessages.DirectoryBuildPropsFileMissed,
                Arguments.DefaultSeverity);
            return;
        }

        var directoryBuildPropsContent = repositoryValidationContext.Repository.ReadAllText(repositorySolutionAccessor.GetDirectoryBuildPropsPath());
        var directoryBuildPropsParser = new DirectoryBuildPropsParser();
        Dictionary<string, string> buildPropsValues = directoryBuildPropsParser.Parse(directoryBuildPropsContent);

        var invalidValues = new List<string>();

        foreach (var (key, value) in request.RequiredKeyValues)
        {
            if (!buildPropsValues.TryGetValue(key, out var specifiedValue))
            {
                invalidValues.Add(key);
                continue;
            }

            if (specifiedValue != value)
                invalidValues.Add(key);
        }

        if (invalidValues.Any())
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"Directory.Build.props options has incorrect value or value is missed: " + invalidValues.ToSingleString(),
                Arguments.DefaultSeverity);
        }
    }
}