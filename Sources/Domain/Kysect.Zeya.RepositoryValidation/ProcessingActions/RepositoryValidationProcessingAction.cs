using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions;

public class RepositoryValidationProcessingAction(IScenarioStepHandler scenarioStepHandler, ILogger<RepositoryValidationProcessingAction> logger)
    : IRepositoryProcessingAction<RepositoryValidationProcessingAction.Request, RepositoryValidationReport>
{
    public record Request(IReadOnlyCollection<IValidationRule> Rules);

    public RepositoryValidationReport Process(ILocalRepository repository, Request request)
    {
        repository.ThrowIfNull();
        request.ThrowIfNull();

        logger.LogDebug("Validate {Repository}", repository.GetRepositoryName());
        var repositoryDiagnosticCollector = new RepositoryDiagnosticCollector(repository.GetRepositoryName());
        var repositoryValidationContext = new RepositoryValidationContext(repository, repositoryDiagnosticCollector);
        var scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);

        var reflectionAttributeFinder = new ReflectionAttributeFinder();
        foreach (IValidationRule scenarioStep in request.Rules)
        {
            var attributeFromType = reflectionAttributeFinder.GetAttributeFromInstance<ScenarioStepAttribute>(scenarioStep);
            logger.LogDebug($"Validate via rule {attributeFromType.ScenarioName}");
            scenarioStepHandler.Handle(scenarioContext, scenarioStep);
        }

        return new RepositoryValidationReport(repositoryValidationContext.DiagnosticCollector.GetDiagnostics(), repositoryValidationContext.DiagnosticCollector.GetRuntimeErrors());
    }
}