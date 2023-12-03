using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;
using System.IO.Abstractions;
using Kysect.Zeya.ManagedDotnetCli;

namespace Kysect.Zeya.ValidationRules.Rules.SourceCode;

public class CentralPackageManagerEnabledValidationRule(IFileSystem fileSystem, DotnetCli dotnetCli) : IScenarioStepExecutor<CentralPackageManagerEnabledValidationRule.Arguments>
{
    [ScenarioStep("SourceCode.CentralPackageManagerEnabled")]
    public record Arguments : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.SourceCode.CentralPackageManagerEnabled;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        var repositoryValidationContext = context.GetValidationContext();

        var selectedProjectDefault = fileSystem
            .Directory
            .EnumerateFiles(repositoryValidationContext.RepositoryAccessor.GetFullPath(), "*.csproj", SearchOption.AllDirectories)
            .FirstOrDefault();

        if (selectedProjectDefault is null)
            return;

        var projectPropertyAccessor = new DotnetProjectPropertyAccessor(selectedProjectDefault, dotnetCli);
        var managePackageVersionsCentrally = projectPropertyAccessor.IsManagePackageVersionsCentrally();
        if (!managePackageVersionsCentrally)
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                $"The repository does not use Central Package Manager",
                Arguments.DefaultSeverity);
        }
    }
}