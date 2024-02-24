using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryValidator(IScenarioStepHandler scenarioStepHandler, ILogger<RepositoryValidator> logger)
{
    public RepositoryValidationReport Validate(ILocalRepository repository, IReadOnlyCollection<IValidationRule> rules)
    {
        repository.ThrowIfNull();
        rules.ThrowIfNull();

        var repositoryDiagnosticCollector = new RepositoryDiagnosticCollector(repository.GetRepositoryName());
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