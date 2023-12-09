using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubRepositoryLicenseValidationRule : IScenarioStepExecutor<GithubRepositoryLicenseValidationRule.Arguments>
{
    [ScenarioStep("Github.RepositoryLicense")]
    public record Arguments(string OwnerName, string Year, string LicenseType) : IScenarioStep
    {
        public static string DiagnosticCode => RuleDescription.Github.RepositoryLicense;
        public static RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments arguments)
    {
        var repositoryValidationContext = context.GetValidationContext();

        if (!repositoryValidationContext.RepositoryAccessor.Exists(ValidationConstants.LicenseFileName))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                $"License file was not found by path {ValidationConstants.LicenseFileName}",
                Arguments.DefaultSeverity);
            return;
        }

        var licenseContent = repositoryValidationContext.RepositoryAccessor.ReadFile(ValidationConstants.LicenseFileName);
        if (!licenseContent.StartsWith(arguments.LicenseType))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                $"License must have header with type {arguments.LicenseType}",
                Arguments.DefaultSeverity);
        }

        var copyrightString = $"Copyright (c) {arguments.Year} {arguments.OwnerName}";
        if (!licenseContent.Contains(copyrightString))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                $"License must contains copyright string: {copyrightString}",
                Arguments.DefaultSeverity);
        }
    }
}