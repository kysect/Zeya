using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace Kysect.Zeya.RepositoryValidation;

public class RepositoryValidationRuleProvider(
    IScenarioSourceProvider scenarioProvider,
    IScenarioSourceCodeParser scenarioSourceCodeParser,
    IScenarioStepParser scenarioStepParser)
{
    public IReadOnlyCollection<IValidationRule> GetValidationRules(string validationScenarioName)
    {
        string scenarioSourceCode = scenarioProvider.GetScenarioSourceCode(validationScenarioName);
        IReadOnlyCollection<ScenarioStepArguments> scenarioStepNodes = scenarioSourceCodeParser.Parse(scenarioSourceCode);
        IReadOnlyCollection<IValidationRule> steps = scenarioStepNodes.Select(scenarioStepParser.ParseScenarioStep).Cast<IValidationRule>().ToList();
        return steps;
    }
}