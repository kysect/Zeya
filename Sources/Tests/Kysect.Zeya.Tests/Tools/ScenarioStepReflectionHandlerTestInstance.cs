﻿using Kysect.ScenarioLib;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Kysect.Zeya.Tests.Tools.Fakes;

namespace Kysect.Zeya.Tests.Tools;

public static class ScenarioStepReflectionHandlerTestInstance
{
    public static ScenarioStepReflectionHandler Create()
    {

        return new ScenarioStepReflectionHandler(new Dictionary<Type, ScenarioStepExecutorReflectionDecorator>
        {
            {typeof(ArtifactsOutputEnabledValidationRule.Arguments), new ScenarioStepExecutorReflectionDecorator(new ArtifactsOutputEnabledValidationRule())},
            {typeof(CentralPackageManagerEnabledValidationRule.Arguments), new ScenarioStepExecutorReflectionDecorator(new CentralPackageManagerEnabledValidationRule())},
            {typeof(ThrowExceptionValidationRule.Arguments), new ScenarioStepExecutorReflectionDecorator(new ThrowExceptionValidationRule())}
        });
    }
}