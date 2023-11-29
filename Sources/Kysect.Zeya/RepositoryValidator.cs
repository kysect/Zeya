using Kysect.CommonLib.Logging;
using Kysect.GithubUtils.RepositorySync;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ValidationRules;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya;

public class RepositoryValidator
{
    private readonly IScenarioSourceProvider _scenarioProvider;
    private readonly IScenarioSourceCodeParser _scenarioSourceCodeParser;
    private readonly IScenarioStepParser _scenarioStepParser;
    private readonly IScenarioStepHandler _scenarioStepHandler;
    private readonly IPathFormatStrategy _pathFormatStrategy;
    private readonly ILogger _logger;

    public RepositoryValidator(
        ILogger logger,
        IScenarioSourceProvider scenarioProvider,
        IScenarioSourceCodeParser scenarioSourceCodeParser,
        IScenarioStepParser scenarioStepParser,
        IScenarioStepHandler scenarioStepHandler,
        IPathFormatStrategy pathFormatStrategy)
    {
        _logger = logger;
        _scenarioProvider = scenarioProvider;
        _scenarioSourceCodeParser = scenarioSourceCodeParser;
        _scenarioStepParser = scenarioStepParser;
        _scenarioStepHandler = scenarioStepHandler;
        _pathFormatStrategy = pathFormatStrategy;
    }
    public RepositoryValidationReport Validate(GithubRepository repository, string validationScenarioName)
    {
        _logger.LogInformation("Validate repository {Url}", repository.FullName);
        _logger.LogTrace("Loading validation configuration");
        var scenarioSourceCode = _scenarioProvider.GetScenarioSourceCode(validationScenarioName);
        IReadOnlyCollection<ScenarioStepArguments> scenarioStepNodes = _scenarioSourceCodeParser.Parse(scenarioSourceCode);
        IReadOnlyCollection<IScenarioStep> steps = scenarioStepNodes.Select(_scenarioStepParser.ParseScenarioStep).ToList();

        var repositoryDiagnosticCollector = new RepositoryDiagnosticCollector(repository.FullName);
        var githubRepositoryAccessor = new GithubRepositoryAccessor(repository, _pathFormatStrategy);
        var repositoryValidationContext = new RepositoryValidationContext(githubRepositoryAccessor, repositoryDiagnosticCollector);
        var scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);

        foreach (IScenarioStep scenarioStep in steps)
        {
            _logger.LogTabDebug(1, $"Validate via rule {scenarioStep.GetType().Name}");
            _scenarioStepHandler.Handle(scenarioContext, scenarioStep);
        }

        return new RepositoryValidationReport(repositoryValidationContext.DiagnosticCollector.GetDiagnostics());
    }
}