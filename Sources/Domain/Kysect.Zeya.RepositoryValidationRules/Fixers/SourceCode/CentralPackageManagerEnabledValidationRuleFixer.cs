﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.Zeya.LocalRepositoryAccess;
using Kysect.Zeya.RepositoryValidation;
using Kysect.Zeya.RepositoryValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.RepositoryValidationRules.Fixers.SourceCode;

public class CentralPackageManagerEnabledValidationRuleFixer(ILogger<CentralPackageManagerEnabledValidationRuleFixer> logger)
    : IValidationRuleFixer<CentralPackageManagerEnabledValidationRule.Arguments>
{
    public void Fix(CentralPackageManagerEnabledValidationRule.Arguments rule, ILocalRepository localRepository)
    {
        rule.ThrowIfNull();
        localRepository.ThrowIfNull();

        LocalRepositorySolution repositorySolutionAccessor = localRepository.SolutionManager.GetSolution();
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        var centralPackageManagementMigrator = new CentralPackageManagementMigrator(logger);
        centralPackageManagementMigrator.Migrate(solutionModifier);
    }
}