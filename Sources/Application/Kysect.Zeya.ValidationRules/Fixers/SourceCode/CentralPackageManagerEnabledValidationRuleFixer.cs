using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.GitIntegration.Abstraction;
using Kysect.Zeya.ValidationRules.Abstractions;
using Kysect.Zeya.ValidationRules.Rules.SourceCode;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class CentralPackageManagerEnabledValidationRuleFixer(XmlDocumentSyntaxFormatter formatter, ILogger logger)
    : IValidationRuleFixer<CentralPackageManagerEnabledValidationRule.Arguments>
{
    public void Fix(CentralPackageManagerEnabledValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        LocalRepositorySolution repositorySolutionAccessor = clonedRepository.SolutionManager.GetSolution();
        DotnetSolutionModifier solutionModifier = repositorySolutionAccessor.GetSolutionModifier();

        var centralPackageManagementMigrator = new CentralPackageManagementMigrator(formatter, logger);
        centralPackageManagementMigrator.Migrate(solutionModifier);
    }
}