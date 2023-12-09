﻿using System.IO.Abstractions;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Microsoft.Extensions.Logging;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class CentralPackageManagerEnabledValidationRuleFixer(IFileSystem fileSystem, ILogger logger) : IValidationRuleFixer
{
    public string DiagnosticCode => RuleDescription.SourceCode.CentralPackageManagerEnabled;

    public void Fix(GithubRepositoryAccessor githubRepository)
    {
        var solutionPath = githubRepository.GetSolutionFilePath();
        var solutionModifier = DotnetSolutionModifier.Create(solutionPath, fileSystem, logger, new SolutionFileParser(logger));
        IReadOnlyCollection<NugetVersion> nugetPackages = CollectNugetIncludes(solutionModifier);

        logger.LogTrace("Apply changes to *.csproj files");
        foreach (var solutionModifierProject in solutionModifier.Projects)
            solutionModifierProject.Accessor.UpdateDocument(ProjectNugetVersionRemover.Instance);

        logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryPackagePropsFileName);
        solutionModifier.DirectoryPackagePropsModifier.Accessor.UpdateDocument(CreateIfNull);
        solutionModifier.DirectoryPackagePropsModifier.Accessor.UpdateDocument(new DirectoryPackagePropsNugetVersionAppender(nugetPackages));

        logger.LogTrace("Saving solution files");
        solutionModifier.Save();
    }

    private IReadOnlyCollection<NugetVersion> CollectNugetIncludes(DotnetSolutionModifier modifier)
    {
        var nugetVersions = new List<NugetVersion>();

        foreach (var dotnetProjectModifier in modifier.Projects)
        {
            foreach (var xmlElementSyntax in dotnetProjectModifier.Accessor.GetNodesByName("PackageReference"))
            {
                var includeAttribute = xmlElementSyntax.GetAttribute("Include");
                if (includeAttribute is null)
                    continue;

                var versionAttribute = xmlElementSyntax.GetAttribute("Version");
                if (versionAttribute is null)
                    continue;

                nugetVersions.Add(new NugetVersion(includeAttribute.Value, versionAttribute.Value));
            }
        }

        return nugetVersions;
    }

    public XmlDocumentSyntax CreateIfNull(XmlDocumentSyntax syntax)
    {
        if (syntax.RootSyntax is not null)
            return syntax;

        var contentTemplate = """
                              <Project>
                                <PropertyGroup>
                                  <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
                                </PropertyGroup>
                                <ItemGroup>
                                </ItemGroup>
                              </Project>
                              """;

        return Parser.ParseText(contentTemplate);
    }
}