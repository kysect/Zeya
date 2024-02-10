using Kysect.DotnetProjectSystem.Parsing;
using Kysect.ScenarioLib;
using Kysect.Zeya.GitIntegration;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using System.IO.Abstractions;

namespace Kysect.Zeya.Tests.Tools;

public static class ScenarioStepReflectionHandlerTestInstance
{
    public static ScenarioStepReflectionHandler Create(IFileSystem fileSystem)
    {
        var repositorySolutionAccessorFactory = new RepositorySolutionAccessorFactory(new SolutionFileContentParser(), fileSystem);

        return new ScenarioStepReflectionHandler(new Dictionary<Type, ScenarioStepExecutorReflectionDecorator>
        {
            {typeof(ArtifactsOutputEnabledValidationRule.Arguments), new ScenarioStepExecutorReflectionDecorator(new ArtifactsOutputEnabledValidationRule(repositorySolutionAccessorFactory))},
            {typeof(CentralPackageManagerEnabledValidationRule.Arguments), new ScenarioStepExecutorReflectionDecorator(new CentralPackageManagerEnabledValidationRule(repositorySolutionAccessorFactory))}
        });
    }
}