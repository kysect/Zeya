using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.LocalRepositoryAccess;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;

public class RepositoryValidationProcessingAction(
    IScenarioStepHandler scenarioStepHandler,
    LoggerRepositoryValidationReporter reporter,
    ILogger<RepositoryValidationProcessingAction> logger)
    : IRepositoryProcessingAction<RepositoryValidationProcessingAction.Request, RepositoryValidationProcessingAction.Response>
{
    public record Request(IReadOnlyCollection<IValidationRule> Rules);
    public record Response();

    public RepositoryProcessingResponse<Response> Process(ILocalRepository repository, Request request)
    {
        repository.ThrowIfNull();
        request.ThrowIfNull();

        logger.LogDebug("Validate {Repository}", repository.GetRepositoryName());
        var repositoryDiagnosticCollector = new RepositoryDiagnosticCollector();
        var repositoryValidationContext = new RepositoryValidationContext(repository, repositoryDiagnosticCollector);
        var scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);

        var reflectionAttributeFinder = new ReflectionAttributeFinder();
        foreach (IValidationRule scenarioStep in request.Rules)
        {
            var attributeFromType = reflectionAttributeFinder.GetAttributeFromInstance<ScenarioStepAttribute>(scenarioStep);
            logger.LogDebug($"Validate via rule {attributeFromType.ScenarioName}");
            scenarioStepHandler.Handle(scenarioContext, scenarioStep);
        }

        var report = new RepositoryValidationReport(repositoryValidationContext.DiagnosticCollector.GetDiagnostics());
        reporter.Report(report, repository.GetRepositoryName());
        return new RepositoryProcessingResponse<Response>("Validation", new Response(), repositoryValidationContext.DiagnosticCollector.GetDiagnostics());
    }
}