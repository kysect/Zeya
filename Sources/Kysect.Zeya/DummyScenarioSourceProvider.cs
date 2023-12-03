using Kysect.ScenarioLib.Abstractions;

namespace Kysect.Zeya;

public class DummyScenarioSourceProvider : IScenarioSourceProvider
{
    public DummyScenarioSourceProvider()
    {
    }

    public IReadOnlyCollection<string> GetScenarioNames()
    {
        return [];
    }

    public string GetScenarioSourceCode(string scenarioName)
    {
        return """
               - Name: Github.RepositoryLicense
                 Parameters:
                   OwnerName: Kysect
                   Year: 2023
                   LicenseType: MIT
               - Name: Github.ReadmeExists
               - Name: Github.BranchProtectionEnabled
                 Parameters:
                   PullRequestReviewRequired: true
                   ConversationResolutionRequired: true
               - Name: Github.AutoBranchDeletionEnabled
               - Name: Github.BuildWorkflowEnabled
                 Parameters:
                   MasterFile: Samples\build-test.yml
               - Name: Github.BuildWorkflowEnabled
                 Parameters:
                   MasterFile: Samples\nuget-publish.yml
               
               - Name: SourceCode.SourcesMustNotBeInRoot
                 Parameters:
                   ExpectedSourceDirectoryName: Sources
               - Name: SourceCode.TargetFrameworkVersionAllowed
                 Parameters:
                   AllowedVersions: [net8.0, netstandard2.0]
               - Name: SourceCode.CentralPackageManagerEnabled
               - Name: SourceCode.CentralPackageManagerVersionSynchronized
                 Parameters:
                   MasterFile: Samples\MasterDirectory.Packages.props
               - Name: SourceCode.RequiredPackagesAdded
                 Parameters:
                   Packages: ["Kysect.Editorconfig"]
               - Name: SourceCode.ArtifactsOutputEnabled
               - Name: DirectoryBuildPropsContainsRequiredFields
                 Parameters:
                   RequiredFields: ["Nullable", "LangVersion", "ImplicitUsings"]

               - Name: Nuget.MetadataSpecified
                 Parameters:
                   RequiredValues: ["PackageIcon", "PackageReadmeFile", "RepositoryUrl", "PublishRepositoryUrl"]
                   RequiredKeyValues:
                     Authors: Kysect
                     Company: Kysect
                     PackageLicenseExpression: MIT
                     DebugType: portable
                     IncludeSymbols: true
                     SymbolPackageFormat: snupkg
               """;
    }
}