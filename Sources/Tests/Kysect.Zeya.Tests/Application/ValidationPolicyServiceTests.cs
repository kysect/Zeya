using FluentAssertions;
using Kysect.Zeya.Application;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Application;

public class ValidationPolicyServiceTests
{
    private readonly ValidationPolicyService _validationPolicyService;
    private readonly PolicyRepositoryService _policyRepositoryService;
    private readonly PolicyService _policyService;

    public ValidationPolicyServiceTests()
    {
        ZeyaDbContext context = ZeyaDbContextTestProvider.CreateContext();
        _validationPolicyService = new ValidationPolicyService(context);
        _policyRepositoryService = new PolicyRepositoryService(new ValidationPolicyRepositoryFactory(), context);
        _policyService = new PolicyService(_validationPolicyService, context);
    }

    [Fact]
    public async Task AddRepository_OnEmptyDatabase_RepositoryAdded()
    {
        ValidationPolicyDto validationPolicyEntity = await _policyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepositoryDto repository = await _policyRepositoryService.AddGithubRepository(validationPolicyEntity.Id, "Owner", "Repository", null);

        IReadOnlyCollection<ValidationPolicyRepositoryDto> repositories = await _policyRepositoryService.GetRepositories(validationPolicyEntity.Id);

        repositories.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllRulesForPolicy_DatabaseWithTwoRepositoryDiagnostic_ReturnDistinctRuleIds()
    {
        ValidationPolicyDto validationPolicyEntity = await _policyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepositoryDto repository = await _policyRepositoryService.AddGithubRepository(validationPolicyEntity.Id, "Owner", "Repository", null);
        var firstDiagnostic = new RepositoryValidationDiagnostic("SRC0001", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var secondDiagnostic = new RepositoryValidationDiagnostic("SRC0002", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var report = new RepositoryValidationReport(new[] { firstDiagnostic, secondDiagnostic }, Array.Empty<RepositoryValidationDiagnostic>());

        await _validationPolicyService.SaveReport(repository.Id, report);

        IReadOnlyCollection<string> rules = await _validationPolicyService.GetAllRulesForPolicy(repository.ValidationPolicyId);

        rules.Should().BeEquivalentTo("SRC0001", "SRC0002", null);
    }

    [Fact]
    public async Task SaveReport_OnEmptyDatabase_ReportSaved()
    {
        ValidationPolicyDto validationPolicyEntity = await _policyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepositoryDto repository = await _policyRepositoryService.AddGithubRepository(validationPolicyEntity.Id, "Owner", "Repository", null);
        var validationDiagnostic = new RepositoryValidationDiagnostic("SRC0001", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var expected = new ValidationPolicyRepositoryDiagnostic(repository.Id, "SRC0001", ValidationPolicyRepositoryDiagnosticSeverity.Warning);
        var report = new RepositoryValidationReport(new[] { validationDiagnostic }, Array.Empty<RepositoryValidationDiagnostic>());

        await _validationPolicyService.SaveReport(repository.Id, report);

        IReadOnlyCollection<ValidationPolicyRepositoryDiagnostic> diagnostics = await _validationPolicyService.GetDiagnostics(repository.Id);

        diagnostics.Should().HaveCount(1);
        diagnostics.First().Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetDiagnosticsTable_ForTwoRepositoriesWithDiagnostics_ReturnTwoElementInListWithExpectedValue()
    {
        ValidationPolicyDto validationPolicyEntity = await _policyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepositoryDto firstRepository = await _policyRepositoryService.AddGithubRepository(validationPolicyEntity.Id, "Owner", "Repository", null);
        ValidationPolicyRepositoryDto secondRepository = await _policyRepositoryService.AddGithubRepository(validationPolicyEntity.Id, "Owner", "Repository2", null);
        var firstDiagnostic = new RepositoryValidationDiagnostic("SRC0001", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var secondDiagnostic = new RepositoryValidationDiagnostic("SRC0002", "Repository", "Message", RepositoryValidationSeverity.Warning);
        var report = new RepositoryValidationReport(new[] { firstDiagnostic, secondDiagnostic }, Array.Empty<RepositoryValidationDiagnostic>());
        var expected = new List<RepositoryDiagnosticTableRow>
            {
                new RepositoryDiagnosticTableRow(firstRepository.Id, "Owner/Repository", new Dictionary<string, string> { ["SRC0001"] = "Warning", ["SRC0002"] = "Warning" }),
                new RepositoryDiagnosticTableRow(secondRepository.Id, "Owner/Repository2", new Dictionary<string, string> { ["SRC0001"] = "Warning", ["SRC0002"] = "Warning" })
            };

        await _validationPolicyService.SaveReport(firstRepository.Id, report);
        await _validationPolicyService.SaveReport(secondRepository.Id, report);

        IReadOnlyCollection<RepositoryDiagnosticTableRow> diagnosticsTable = await _validationPolicyService.GetDiagnosticsTable(validationPolicyEntity.Id);
        diagnosticsTable.Should().BeEquivalentTo(expected);
    }
}
