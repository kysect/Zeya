﻿using Kysect.CommonLib.BaseTypes.Extensions;
using Kysect.DotnetProjectSystem.Projects;
using Kysect.DotnetProjectSystem.SolutionModification;
using Kysect.DotnetProjectSystem.Xml;
using Kysect.Zeya.Abstractions.Contracts;
using Kysect.Zeya.ValidationRules.Rules.Nuget;
using Microsoft.Extensions.Logging;

namespace Kysect.Zeya.ValidationRules.Fixers.Nuget;

public class NugetMetadataHaveCorrectValueValidationRuleFixer(
    DotnetSolutionModifierFactory dotnetSolutionModifierFactory,
    RepositorySolutionAccessorFactory repositorySolutionAccessorFactory,
    XmlDocumentSyntaxFormatter formatter,
    ILogger logger) : IValidationRuleFixer<NugetMetadataHaveCorrectValueValidationRule.Arguments>
{
    public void Fix(NugetMetadataHaveCorrectValueValidationRule.Arguments rule, IClonedRepository clonedRepository)
    {
        rule.ThrowIfNull();
        clonedRepository.ThrowIfNull();

        RepositorySolutionAccessor repositorySolutionAccessor = repositorySolutionAccessorFactory.Create(clonedRepository);
        string solutionPath = repositorySolutionAccessor.GetSolutionFilePath();
        DotnetSolutionModifier solutionModifier = dotnetSolutionModifierFactory.Create(solutionPath);

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryBuildPropsFileName);

        DirectoryBuildPropsFile directoryBuildPropsFile = solutionModifier.GetOrCreateDirectoryBuildPropsModifier();
        foreach ((string key, string value) in rule.RequiredKeyValues)
        {
            logger.LogDebug("Set {Key} to {Value}", key, value);
            directoryBuildPropsFile.File.Properties.SetProperty(key, value);
        }

        // TODO: force somehow saving
        solutionModifier.Save(formatter);
    }
}