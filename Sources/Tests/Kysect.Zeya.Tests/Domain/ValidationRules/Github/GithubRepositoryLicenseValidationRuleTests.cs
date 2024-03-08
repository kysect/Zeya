using Kysect.Zeya.RepositoryValidationRules.Rules.Github;
using Kysect.Zeya.Tests.Tools;
using System.IO.Abstractions.TestingHelpers;

namespace Kysect.Zeya.Tests.Domain.ValidationRules.Github;

public class GithubRepositoryLicenseValidationRuleTests
{
    private readonly ValidationTestFixture _validationTestFixture;
    private readonly GithubRepositoryLicenseValidationRule _validationRule;

    public GithubRepositoryLicenseValidationRuleTests()
    {
        _validationTestFixture = new ValidationTestFixture();
        _validationRule = new GithubRepositoryLicenseValidationRule();
    }

    [Fact]
    public void Execute_EmptyRepository_ReturnDiagnosticAboutMissedLicenseFile()
    {
        var arguments = new GithubRepositoryLicenseValidationRule.Arguments("owner", "2024", "MIT");

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "License file was not found by path LICENSE");
    }

    [Fact]
    public void Execute_LicenseDoNotStartWithLicenseType_ReturnDiagnosticAboutIncorrectLicenseType()
    {
        var arguments = new GithubRepositoryLicenseValidationRule.Arguments("owner", "2024", "MIT");
        var licenseFileContent = """
                                 This is some custom text. Looks like license, isn't?
                                 Copyright (c) 2024 owner
                                 """;
        _validationTestFixture.FileSystem.AddFile("LICENSE", new MockFileData(licenseFileContent));

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "License must have header with type MIT");
    }

    [Fact]
    public void Execute_LicenseEmptyFile_ReturnTwoDiagnostic()
    {
        var arguments = new GithubRepositoryLicenseValidationRule.Arguments("owner", "2024", "MIT");
        var licenseFileContent = string.Empty;
        _validationTestFixture.FileSystem.AddFile("LICENSE", new MockFileData(licenseFileContent));

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(2)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "License must have header with type MIT")
            .ShouldHaveDiagnostic(2, arguments.DiagnosticCode, "License must contains copyright string: Copyright (c) 2024 owner");
    }

    [Fact]
    public void Execute_LicenseWithIncorrectYear_ReturnDiagnosticAboutIncorrectHeader()
    {
        var arguments = new GithubRepositoryLicenseValidationRule.Arguments("owner", "2024", "MIT");
        var licenseFileContent = """
                                 MIT
                                 Copyright (c) 2023 owner
                                 """;
        _validationTestFixture.FileSystem.AddFile("LICENSE", new MockFileData(licenseFileContent));

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "License must contains copyright string: Copyright (c) 2024 owner");
    }

    [Fact]
    public void Execute_LicenseWithIncorrectOwner_ReturnDiagnosticAboutIncorrectHeader()
    {
        var arguments = new GithubRepositoryLicenseValidationRule.Arguments("owner", "2024", "MIT");
        var licenseFileContent = """
                                 MIT
                                 Copyright (c) 2024 other owner
                                 """;
        _validationTestFixture.FileSystem.AddFile("LICENSE", new MockFileData(licenseFileContent));

        _validationRule.Execute(_validationTestFixture.CreateGithubRepositoryValidationScenarioContext(), arguments);

        _validationTestFixture.DiagnosticCollectorAsserts
            .ShouldHaveDiagnosticCount(1)
            .ShouldHaveDiagnostic(1, arguments.DiagnosticCode, "License must contains copyright string: Copyright (c) 2024 owner");
    }
}