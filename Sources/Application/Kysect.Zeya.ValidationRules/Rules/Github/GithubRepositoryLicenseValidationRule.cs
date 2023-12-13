using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.Abstractions.Models;

namespace Kysect.Zeya.ValidationRules.Rules.Github;

public class GithubRepositoryLicenseValidationRule : IScenarioStepExecutor<GithubRepositoryLicenseValidationRule.Arguments>
{
    [ScenarioStep("Github.RepositoryLicense")]
    public record Arguments(string OwnerName, string Year, string LicenseType) : IValidationRule
    {
        public string DiagnosticCode => RuleDescription.Github.RepositoryLicense;
        public const RepositoryValidationSeverity DefaultSeverity = RepositoryValidationSeverity.Warning;
    }

    public void Execute(ScenarioContext context, Arguments request)
    {
        context.ThrowIfNull();
        request.ThrowIfNull();

        var repositoryValidationContext = context.GetValidationContext();

        if (!repositoryValidationContext.RepositoryAccessor.Exists(ValidationConstants.LicenseFileName))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"License file was not found by path {ValidationConstants.LicenseFileName}",
                Arguments.DefaultSeverity);
            return;
        }

        var licenseContent = repositoryValidationContext.RepositoryAccessor.ReadFile(ValidationConstants.LicenseFileName);
        if (!licenseContent.StartsWith(request.LicenseType))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"License must have header with type {request.LicenseType}",
                Arguments.DefaultSeverity);
        }

        var copyrightString = $"Copyright (c) {request.Year} {request.OwnerName}";
        if (!licenseContent.Contains(copyrightString))
        {
            repositoryValidationContext.DiagnosticCollector.Add(
                request.DiagnosticCode,
                $"License must contains copyright string: {copyrightString}",
                Arguments.DefaultSeverity);
        }
    }
}