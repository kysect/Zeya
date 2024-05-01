using FluentAssertions;
using Kysect.Zeya.Application.DatabaseQueries;
using Kysect.Zeya.Application.LocalHandling;
using Kysect.Zeya.Application.Repositories;
using Kysect.Zeya.DataAccess.Abstractions;
using Kysect.Zeya.DataAccess.EntityFramework;
using Kysect.Zeya.Dtos;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.ProcessingActions;
using Kysect.Zeya.RepositoryValidation.ProcessingActions.Validation;
using Kysect.Zeya.Tests.Tools;

namespace Kysect.Zeya.Tests.Application;

public class ValidationPolicyDatabaseQueriesTests
{
    private readonly ValidationPolicyDatabaseQueries _validationPolicyDatabaseQueries;
    private readonly PolicyRepositoryService _policyRepositoryService;
    private readonly PolicyService _policyService;

    public ValidationPolicyDatabaseQueriesTests()
    {
        ZeyaDbContext context = ZeyaDbContextTestProvider.CreateContext();
        _validationPolicyDatabaseQueries = new ValidationPolicyDatabaseQueries(context);
        _policyRepositoryService = new PolicyRepositoryService(new ValidationPolicyRepositoryFactory(), context);
        _policyService = new PolicyService(_validationPolicyDatabaseQueries, context);
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
        var firstDiagnostic = new RepositoryProcessingMessage("SRC0001", "Message", RepositoryValidationSeverity.Warning);
        var secondDiagnostic = new RepositoryProcessingMessage("SRC0002", "Message", RepositoryValidationSeverity.Warning);
        RepositoryProcessingMessage[] repositoryProcessingMessages = new[] { firstDiagnostic, secondDiagnostic };

        await _validationPolicyDatabaseQueries.SaveReport(repository.Id, repositoryProcessingMessages);

        IReadOnlyCollection<string> rules = await _validationPolicyDatabaseQueries.GetAllRulesForPolicy(repository.ValidationPolicyId);

        rules.Should().BeEquivalentTo("SRC0001", "SRC0002");
    }

    [Fact]
    public async Task SaveReport_OnEmptyDatabase_ReportSaved()
    {
        ValidationPolicyDto validationPolicyEntity = await _policyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepositoryDto repository = await _policyRepositoryService.AddGithubRepository(validationPolicyEntity.Id, "Owner", "Repository", null);
        var validationDiagnostic = new RepositoryProcessingMessage("SRC0001", "Message", RepositoryValidationSeverity.Warning);
        var expected = new ValidationPolicyRepositoryDiagnostic(repository.Id, "SRC0001", ValidationPolicyRepositoryDiagnosticSeverity.Warning);
        RepositoryProcessingMessage[] repositoryProcessingMessages = new[] { validationDiagnostic };

        await _validationPolicyDatabaseQueries.SaveReport(repository.Id, repositoryProcessingMessages);

        IReadOnlyCollection<ValidationPolicyRepositoryDiagnostic> diagnostics = await _validationPolicyDatabaseQueries.GetDiagnostics(repository.Id);

        diagnostics.Should().HaveCount(1);
        diagnostics.First().Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetDiagnosticsTable_ForTwoRepositoriesWithDiagnostics_ReturnTwoElementInListWithExpectedValue()
    {
        ValidationPolicyDto validationPolicyEntity = await _policyService.CreatePolicy("Policy", "Content");
        ValidationPolicyRepositoryDto firstRepository = await _policyRepositoryService.AddGithubRepository(validationPolicyEntity.Id, "Owner", "Repository", null);
        ValidationPolicyRepositoryDto secondRepository = await _policyRepositoryService.AddGithubRepository(validationPolicyEntity.Id, "Owner", "Repository2", null);
        var firstDiagnostic = new RepositoryProcessingMessage("SRC0001", "Message", RepositoryValidationSeverity.Warning);
        var secondDiagnostic = new RepositoryProcessingMessage("SRC0002", "Message", RepositoryValidationSeverity.Warning);
        var report = new RepositoryValidationReport(new[] { firstDiagnostic, secondDiagnostic });
        var expected = new List<RepositoryDiagnosticTableRow>
            {
                new RepositoryDiagnosticTableRow(firstRepository.Id, "Owner/Repository", new Dictionary<string, string> { ["SRC0001"] = "Warning", ["SRC0002"] = "Warning" }),
                new RepositoryDiagnosticTableRow(secondRepository.Id, "Owner/Repository2", new Dictionary<string, string> { ["SRC0001"] = "Warning", ["SRC0002"] = "Warning" })
            };

        await _validationPolicyDatabaseQueries.SaveReport(firstRepository.Id, report.Diagnostics);
        await _validationPolicyDatabaseQueries.SaveReport(secondRepository.Id, report.Diagnostics);

        IReadOnlyCollection<RepositoryDiagnosticTableRow> diagnosticsTable = await _validationPolicyDatabaseQueries.GetDiagnosticsTable(validationPolicyEntity.Id);
        diagnosticsTable.Should().BeEquivalentTo(expected);
    }
}
