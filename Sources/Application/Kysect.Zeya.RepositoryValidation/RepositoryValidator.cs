using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.CommonLib.Reflection;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Rules;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryValidator(
    ILogger logger,
    IScenarioSourceProvider scenarioProvider,
    IScenarioSourceCodeParser scenarioSourceCodeParser,
    IScenarioStepParser scenarioStepParser,
    IScenarioStepHandler scenarioStepHandler,
    GithubRepositoryAccessorFactory githubRepositoryAccessorFactory)
{
    public RepositoryValidationReport Validate(GithubRepository repository, string validationScenarioName)
    {
        repository.ThrowIfNull();
        validationScenarioName.ThrowIfNull();

        logger.LogInformation("Validate repository {Url}", repository.FullName);
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

    public RepositoryValidationReport Validate(GithubRepository repository, IReadOnlyCollection<IValidationRule> rules)
    {
        repository.ThrowIfNull();
        rules.ThrowIfNull();

        var repositoryDiagnosticCollector = new RepositoryDiagnosticCollector(repository.FullName);
        var githubRepositoryAccessor = githubRepositoryAccessorFactory.Create(repository);
        var repositoryValidationContext = RepositoryValidationContext.CreateForGitHub(repository, githubRepositoryAccessor, repositoryDiagnosticCollector);
        var scenarioContext = RepositoryValidationContextExtensions.CreateScenarioContext(repositoryValidationContext);

        var reflectionAttributeFinder = new ReflectionAttributeFinder();
        foreach (IValidationRule scenarioStep in rules)
        {
            var attributeFromType = reflectionAttributeFinder.GetAttributeFromInstance<ScenarioStepAttribute>(scenarioStep);
            logger.LogDebug($"Validate via rule {attributeFromType.ScenarioName}");
            scenarioStepHandler.Handle(scenarioContext, scenarioStep);
        }

        return new RepositoryValidationReport(repositoryValidationContext.DiagnosticCollector.GetDiagnostics());
    }
}