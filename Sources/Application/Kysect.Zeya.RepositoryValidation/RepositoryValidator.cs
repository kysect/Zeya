using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.RepositoryValidation.Models;
using Kysect.Zeya.ValidationRules.Abstractions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryValidator(ILogger logger, IScenarioStepHandler scenarioStepHandler)
{
    public RepositoryValidationReport Validate(ClonedGithubRepositoryAccessor repository, IReadOnlyCollection<IValidationRule> rules)
    {
        repository.ThrowIfNull();
        rules.ThrowIfNull();

        var repositoryDiagnosticCollector = new RepositoryDiagnosticCollector(repository.GithubMetadata.FullName);
        var repositoryValidationContext = new RepositoryValidationContext(repository, repositoryDiagnosticCollector);
        var scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);

        var reflectionAttributeFinder = new ReflectionAttributeFinder();
        foreach (IValidationRule scenarioStep in rules)
        {
            var attributeFromType = reflectionAttributeFinder.GetAttributeFromInstance<ScenarioStepAttribute>(scenarioStep);
            logger.LogDebug($"Validate via rule {attributeFromType.ScenarioName}");
            scenarioStepHandler.Handle(scenarioContext, scenarioStep);
        }

        return new RepositoryValidationReport(repositoryValidationContext.DiagnosticCollector.GetDiagnostics(), repositoryValidationContext.DiagnosticCollector.GetRuntimeErrors());
    }
}