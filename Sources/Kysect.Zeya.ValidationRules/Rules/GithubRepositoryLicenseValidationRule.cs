using System.IO.Abstractions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules;

public class GithubRepositoryLicenseValidationRule(IFileSystem fileSystem) : IScenarioStepExecutor<GithubRepositoryLicenseValidationRule.Arguments>
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

        var pathToLicenseFile = Path.Combine(repositoryValidationContext.RepositoryAccessor.GetFullPath(), ValidationConstants.LicenseFileName);
        if (!fileSystem.File.Exists(pathToLicenseFile))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                Arguments.DiagnosticCode,
                $"License file was not found by path {pathToLicenseFile}",
                Arguments.DefaultSeverity);
            return;
        }

        var licenseContent = fileSystem.File.ReadAllText(pathToLicenseFile);
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