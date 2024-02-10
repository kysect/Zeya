﻿using FluentAssertions;
using Kysect.ScenarioLib;
using Kysect.ScenarioLib.YamlParser;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.ValidationRules;
using Kysect.Zeya.ValidationRules.Abstractions;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.RepositoryValidation;

public class RepositoryValidationRuleProviderTests
{
    private readonly RepositoryValidationRuleProvider _repositoryValidationRuleProvider;

    public RepositoryValidationRuleProviderTests()
    {
        var fileSystem = new FileSystem();
        var scenarioSourceProvider = new ScenarioSourceProvider(fileSystem.Path.Combine("Tools", "Assets"));
        var yamlScenarioSourceCodeParser = new YamlScenarioSourceCodeParser();
        var scenarioStepReflectionParser = ScenarioStepReflectionParser.Create(typeof(RuleDescription).Assembly);
        _repositoryValidationRuleProvider = new RepositoryValidationRuleProvider(scenarioSourceProvider, yamlScenarioSourceCodeParser, scenarioStepReflectionParser);
    }

    [Fact]
    public void GetValidationRules_ForDemoScenario_ReturnExpectedRuleCount()
    {
        IReadOnlyCollection<IValidationRule> rules = _repositoryValidationRuleProvider.GetValidationRules("ValidationScenario.yaml");

        rules.Should().HaveCount(14);
    }
}