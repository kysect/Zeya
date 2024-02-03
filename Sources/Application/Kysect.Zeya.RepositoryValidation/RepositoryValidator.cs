using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.RepositoryAccess;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryValidator(
    ILogger logger,
    IScenarioSourceProvider scenarioProvider,
    IScenarioSourceCodeParser scenarioSourceCodeParser,
    IScenarioStepParser scenarioStepParser,
    IScenarioStepHandler scenarioStepHandler)
{
    public RepositoryValidationReport Validate(ClonedGithubRepositoryAccessor repository, string validationScenarioName)
    {
        repository.ThrowIfNull();
        validationScenarioName.ThrowIfNull();

        logger.LogInformation("Validate repository {Url}", repository.GithubMetadata.FullName);
        IReadOnlyCollection<IValidationRule> steps = GetValidationRules(validationScenarioName);
        return Validate(repository, steps);
    }

    public IReadOnlyCollection<IValidationRule> GetValidationRules(string validationScenarioName)
    {
        logger.LogTrace("Loading validation configuration");
        var scenarioSourceCode = scenarioProvider.GetScenarioSourceCode(validationScenarioName);
        IReadOnlyCollection<ScenarioStepArguments> scenarioStepNodes = scenarioSourceCodeParser.Parse(scenarioSourceCode);
        IReadOnlyCollection<IValidationRule> steps = scenarioStepNodes.Select(scenarioStepParser.ParseScenarioStep).Cast<IValidationRule>().ToList();
        return steps;
    }

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