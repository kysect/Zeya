using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.GithubUtils.RepositorySync;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Rules;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace Kysect.Zeya.RepositoryValidation;

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
        repository.ThrowIfNull();
        validationScenarioName.ThrowIfNull();

        _logger.LogInformation("Validate repository {Url}", repository.FullName);
        IReadOnlyCollection<IValidationRule> steps = GetValidationRules(validationScenarioName);
        return Validate(repository, steps);
    }

    public IReadOnlyCollection<IValidationRule> GetValidationRules(string validationScenarioName)
    {
        _logger.LogTrace("Loading validation configuration");
        var scenarioSourceCode = _scenarioProvider.GetScenarioSourceCode(validationScenarioName);
        IReadOnlyCollection<ScenarioStepArguments> scenarioStepNodes = _scenarioSourceCodeParser.Parse(scenarioSourceCode);
        IReadOnlyCollection<IValidationRule> steps = scenarioStepNodes.Select(_scenarioStepParser.ParseScenarioStep).Cast<IValidationRule>().ToList();
        return steps;
    }

    public RepositoryValidationReport Validate(GithubRepository repository, IReadOnlyCollection<IValidationRule> rules)
    {
        repository.ThrowIfNull();
        rules.ThrowIfNull();

        var repositoryDiagnosticCollector = new RepositoryDiagnosticCollector(repository.FullName);
        var githubRepositoryAccessor = new GithubRepositoryAccessor(repository, _pathFormatStrategy, _fileSystem);
        var repositoryValidationContext = new RepositoryValidationContext(githubRepositoryAccessor, repositoryDiagnosticCollector);
        var scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);

        var reflectionAttributeFinder = new ReflectionAttributeFinder();
        foreach (IValidationRule scenarioStep in rules)
        {
            var attributeFromType = reflectionAttributeFinder.GetAttributeFromInstance<ScenarioStepAttribute>(scenarioStep);
            _logger.LogDebug($"Validate via rule {attributeFromType.ScenarioName}");
            _scenarioStepHandler.Handle(scenarioContext, scenarioStep);
        }

        return new RepositoryValidationReport(repositoryValidationContext.DiagnosticCollector.GetDiagnostics());
    }
}