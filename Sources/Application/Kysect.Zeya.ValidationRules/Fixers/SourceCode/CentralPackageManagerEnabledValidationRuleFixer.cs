using System.IO.Abstractions;
using Kysect.DotnetSlnParser.Modifiers;
using Kysect.DotnetSlnParser.Parsers;
using Kysect.Zeya.Abstractions;
using Kysect.Zeya.GithubIntegration;
using Kysect.Zeya.ProjectSystemIntegration;
using Kysect.Zeya.ProjectSystemIntegration.XmlProjectFileModifyStrategies;
using Microsoft.Extensions.Logging;
using Microsoft.Language.Xml;

namespace Kysect.Zeya.ValidationRules.Fixers.SourceCode;

public class CentralPackageManagerEnabledValidationRuleFixer : IValidationRuleFixer
{
    public string DiagnosticCode => RuleDescription.SourceCode.CentralPackageManagerEnabled;

    private readonly IFileSystem _fileSystem;
    private readonly ILogger _logger;

    public CentralPackageManagerEnabledValidationRuleFixer(IFileSystem fileSystem, ILogger logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public void Fix(GithubRepositoryAccessor githubRepository)
    {
        var repositoryPath = githubRepository.GetFullPath();

        var solutions = _fileSystem.Directory.EnumerateFiles(repositoryPath, "*.sln", SearchOption.AllDirectories).ToList();
        if (solutions.Count == 0)
            throw new ZeyaException($"Repository {repositoryPath} does not contains .sln files");
        
        // TODO: investigate this path
        if (solutions.Count > 1)
            throw new ZeyaException($"CPM Code fixer does not support repositories with more than one solution file.");

        var solutionModifier = DotnetSolutionModifier.Create(solutions.Single(), _fileSystem, _logger, new SolutionFileParser(_logger));
        IReadOnlyCollection<NugetVersion> nugetPackages = CollectNugetIncludes(solutionModifier);

        _logger.LogTrace("Apply changes to *.csproj files");
        foreach (var solutionModifierProject in solutionModifier.Projects)
            solutionModifierProject.Accessor.UpdateDocument(ProjectNugetVersionRemover.Instance);

        _logger.LogTrace("Apply changes to {FileName} file", ValidationConstants.DirectoryPackagePropsFileName);
        solutionModifier.DirectoryPackagePropsModifier.Accessor.UpdateDocument(CreateIfNull);
        solutionModifier.DirectoryPackagePropsModifier.Accessor.UpdateDocument(new DirectoryPackagePropsNugetVersionAppender(nugetPackages));

        _logger.LogTrace("Saving solution files");
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