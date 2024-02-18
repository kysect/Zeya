﻿using FluentAssertions;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools.Fakes;

namespace Kysect.Zeya.Tests.Domain.RepositoryValidation;

public class RepositoryValidationRuleProviderTests
{
    private readonly RepositoryValidationRuleProvider _repositoryValidationRuleProvider;

    public RepositoryValidationRuleProviderTests()
    {
        _repositoryValidationRuleProvider = RepositoryValidationRuleProviderTestInstance.Create();
    }

    [Fact]
    public void GetValidationRules_ForDemoScenario_ReturnExpectedRuleCount()
    {
        IReadOnlyCollection<IValidationRule> rules = _repositoryValidationRuleProvider.GetValidationRules("ValidationScenario.yaml");

        rules.Should().HaveCount(14);
    }
}