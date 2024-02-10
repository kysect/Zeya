using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.ScenarioLib.Abstractions;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Models;
using Kysect.Zeya.ValidationRules.Abstractions;

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

        if (!repositoryValidationContext.Repository.Exists(ValidationConstants.LicenseFileName))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"License file was not found by path {ValidationConstants.LicenseFileName}",
                Arguments.DefaultSeverity);
            return;
        }

        var licenseContent = repositoryValidationContext.Repository.ReadAllText(ValidationConstants.LicenseFileName);
        if (!licenseContent.StartsWith(request.LicenseType))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"License must have header with type {request.LicenseType}",
                Arguments.DefaultSeverity);
        }

        var copyrightString = $"Copyright (c) {request.Year} {request.OwnerName}";
        if (!licenseContent.Contains(copyrightString))
        {
            repositoryValidationContext.DiagnosticCollector.AddDiagnostic(
                request.DiagnosticCode,
                $"License must contains copyright string: {copyrightString}",
                Arguments.DefaultSeverity);
        }
    }
}