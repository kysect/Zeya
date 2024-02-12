using Kysect.ScenarioLib.YamlParser;
using Kysect.ScenarioLib;
using Kysect.Zeya.RepositoryValidation;
using System.IO.Abstractions;
using Kysect.Zeya.RepositoryValidationRules.Rules;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public static class RepositoryValidationRuleProviderTestInstance
{
    public static RepositoryValidationRuleProvider Create()
    {
        var fileSystem = new FileSystem();
        var scenarioSourceProvider = new ScenarioSourceProvider(fileSystem, fileSystem.Path.Combine("Tools", "Assets"));
        var yamlScenarioSourceCodeParser = new YamlScenarioSourceCodeParser();
        var scenarioStepReflectionParser = ScenarioStepReflectionParser.Create(typeof(RuleDescription).Assembly);
        return new RepositoryValidationRuleProvider(scenarioSourceProvider, yamlScenarioSourceCodeParser, scenarioStepReflectionParser);
    }
}