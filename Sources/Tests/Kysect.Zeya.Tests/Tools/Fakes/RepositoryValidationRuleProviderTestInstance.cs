using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.YamlParser;
using Kysect.ScenarioLib;
using Kysect.Zeya.RepositoryValidation;
using System.IO.Abstractions;
using Kysect.Zeya.RepositoryValidationRules.Rules;

namespace Kysect.Zeya.Tests.Tools.Fakes;

public static class RepositoryValidationRuleProviderTestInstance
{
    public static ScenarioContentProvider CreateContentProvider(IFileSystem fileSystem)
    {
        fileSystem.ThrowIfNull();
        return new ScenarioContentProvider(fileSystem, fileSystem.Path.Combine("Tools", "Assets"));
    }

    public static ValidationRuleParser Create()
    {
        var yamlScenarioSourceCodeParser = new YamlScenarioContentParser();
        var scenarioStepReflectionParser = ScenarioContentStepReflectionDeserializer.Create(typeof(RuleDescription).Assembly);
        return new ValidationRuleParser(new ScenarioContentDeserializer(yamlScenarioSourceCodeParser, scenarioStepReflectionParser));
    }
}