using System.IO.Abstractions;
using Kysect.CommonLib.Reflection;
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
    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public RepositoryValidator(
        ILogger logger,
        IScenarioSourceProvider scenarioProvider,
        IScenarioSourceCodeParser scenarioSourceCodeParser,
        IScenarioStepParser scenarioStepParser,
        IScenarioStepHandler scenarioStepHandler,
        IPathFormatStrategy pathFormatStrategy,
        IFileSystem fileSystem)
    {
        _logger = logger;
        _scenarioProvider = scenarioProvider;
        _scenarioSourceCodeParser = scenarioSourceCodeParser;
        _scenarioStepParser = scenarioStepParser;
        _scenarioStepHandler = scenarioStepHandler;
        _pathFormatStrategy = pathFormatStrategy;
        _fileSystem = fileSystem;
    }
    public RepositoryValidationReport Validate(GithubRepository repository, string validationScenarioName)
    {
        _logger.LogInformation("Validate repository {Url}", repository.FullName);
        _logger.LogTrace("Loading validation configuration");
        var scenarioSourceCode = _scenarioProvider.GetScenarioSourceCode(validationScenarioName);
        IReadOnlyCollection<ScenarioStepArguments> scenarioStepNodes = _scenarioSourceCodeParser.Parse(scenarioSourceCode);
        IReadOnlyCollection<IScenarioStep> steps = scenarioStepNodes.Select(_scenarioStepParser.ParseScenarioStep).ToList();

        var repositoryDiagnosticCollector = new RepositoryDiagnosticCollector(repository.FullName);
        var githubRepositoryAccessor = new GithubRepositoryAccessor(repository, _pathFormatStrategy, _fileSystem);
        var repositoryValidationContext = new RepositoryValidationContext(githubRepositoryAccessor, repositoryDiagnosticCollector);
        var scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);

        var reflectionAttributeFinder = new ReflectionAttributeFinder();
        foreach (IScenarioStep scenarioStep in steps)
        {
            var attributeFromType = reflectionAttributeFinder.GetAttributeFromInstance<ScenarioStepAttribute>(scenarioStep);
            _logger.LogDebug($"Validate via rule {attributeFromType.ScenarioName}");
            _scenarioStepHandler.Handle(scenarioContext, scenarioStep);
        }

        return new RepositoryValidationReport(repositoryValidationContext.DiagnosticCollector.GetDiagnostics());
    }
}