﻿using Kysect.TerminalUserInterface.Commands;
using Kysect.Zeya.GithubIntegration.Abstraction;
using Kysect.Zeya.GithubIntegration.Abstraction.Models;
using Kysect.Zeya.IntegrationManager;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidation.Models;
using Kysect.Zeya.Tui.Controls;
using Kysect.Zeya.ValidationRules.Abstractions;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.Tui.Commands;

public class AnalyzeRepositoryCommand(
    RepositoryValidator repositoryValidator,
    IRepositoryValidationReporter reporter,
    ILogger logger,
    RepositoryValidationRuleProvider validationRuleProvider,
    IGithubRepositoryProvider githubRepositoryProvider)
    : ITuiCommand
{
    public string Name => "Analyze repository";

    public void Execute()
    {
        GithubRepository githubRepository = RepositoryInputControl.Ask();
        ClonedGithubRepositoryAccessor githubRepositoryAccessor = githubRepositoryProvider.GetGithubRepository(githubRepository.Owner, githubRepository.Name);

        logger.LogInformation("Validate repository {Url}", githubRepositoryAccessor.GithubMetadata.FullName);
        logger.LogTrace("Loading validation configuration");
        // TODO: remove hardcoded value
        IReadOnlyCollection<IValidationRule> steps = validationRuleProvider.GetValidationRules(@"Demo-validation.yaml");
        RepositoryValidationReport report = repositoryValidator.Validate(githubRepositoryAccessor, steps);
        reporter.Report(report);
    }
}