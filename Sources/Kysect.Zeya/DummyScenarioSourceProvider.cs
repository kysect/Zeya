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
               
               - Name: SourceCodeDirectoryExists
                 Parameters:
                   ExpectedSourceDirectoryName: Sources
               - Name: CentralPackageManagerEnabled
               - Name: ValidateTargetFrameworkVersion
                 Parameters:
                   AllowedVersions: [net8.0, netstandard2.0]
               - Name: ValidateCentralPackageManagerConfig
                 Parameters:
                   MasterFile: Samples\MasterDirectory.Packages.props
               - Name: ContainsRequiredFile
                 Parameters:
                   FilePath: LICENSE
                   Sample: Samples\LICENSE
               - Name: ContainsRequiredFile
                 Parameters:
                   FilePath: .github\workflows\build-test.yml
                   Sample: Samples\build-test.yml
               - Name: ContainsRequiredFile
                 Parameters:
                   FilePath: .github\workflows\nuget-publish.yml
                   Sample: Samples\nuget-publish.yml
               - Name: DirectoryBuildPropsContainsRequiredFields
                 Parameters:
                   RequiredFields: [
                        "Authors",
                        "Company",
                        "PackageReadmeFile",
                        "PackageLicenseFile",
                        "Nullable",
                        "LangVersion",
                        "ImplicitUsings",
                        "RepositoryUrl",
                        "Version",
                        "UseArtifactsOutput",
                        "PublishRepositoryUrl",
                        "IncludeSymbols",
                        "SymbolPackageFormat",
                        "EmbedUntrackedSources",
                        "PackageIcon",
                        "PackageLicenseExpression"
                   ]
               - Name: RequiredPackagesAdded
                 Parameters:
                   Packages: ["Kysect.Editorconfig"]
               - Name: GithubBranchProtectionEnabled
                 Parameters:
                   PullRequestReviewRequired: true
                   ConversationResolutionRequired: true
               """;
    }
}